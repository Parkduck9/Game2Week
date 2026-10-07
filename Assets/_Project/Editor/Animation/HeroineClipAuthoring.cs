using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game2Week.EditorTools.Animation
{
    /// <summary>
    /// 주인공 v2 동작 10종을 자세 함수로 다시 만든다 (8.5단계).
    /// glb에 들어 있던 클립은 팔다리 회전이 거의 0이라 기본(T자에 가까운) 자세가 그대로 보였다.
    /// 자세는 캐릭터 공간(앞·오른쪽·위) 각도로 정의하고, 모델의 앞·오른쪽 방향은 눈·팔 위치로 자동 판별한다.
    /// 실행: Tools ▸ Heroine ▸ 동작 클립 다시 만들기 (또는 -executeMethod Game2Week.EditorTools.Animation.HeroineClipAuthoring.BuildFromCommandLine)
    /// </summary>
    public static class HeroineClipAuthoring
    {
        const string PrefabPath = "Assets/_Project/Prefabs/Battle/Player_Heroine.prefab";
        const string ControllerPath = "Assets/_Project/Art/Characters/Heroine/HeroineV2.controller";
        const string ClipFolder = "Assets/_Project/Art/Characters/Heroine/Clips";
        const float FrameRate = 30f;

        /// <summary>팔 하나: 위팔 앞으로 든 각도·바깥으로 벌린 각도, 팔꿈치 굽힘 (도).</summary>
        struct Arm { public float forward, outward, elbow; public Arm(float f, float o, float e) { forward = f; outward = o; elbow = e; } }
        /// <summary>다리 하나: 허벅지 앞으로 든 각도, 무릎 굽힘, 바깥 벌림 (도).</summary>
        struct Leg { public float forward, knee, outward; public Leg(float f, float k, float o = 3f) { forward = f; knee = k; outward = o; } }

        sealed class Pose
        {
            public float bob;          // 엉덩이 높이 변화 (엉덩이 높이 대비 비율, 다리 굽힘으로 내려가는 양은 자동)
            public bool grounded = true;
            public float hipLean, spineLean, spineTwist, sideLean, headNod, tailSway;
            public Arm right = new(4f, 10f, 14f), left = new(4f, 10f, 14f);
            public Leg rightLeg = new(0f, 0f), leftLeg = new(0f, 0f);
        }

        sealed class ClipSpec
        {
            public string name; public float length; public bool loop; public Func<float, Pose> pose;
            public ClipSpec(string n, float l, bool lp, Func<float, Pose> p) { name = n; length = l; loop = lp; pose = p; }
        }

        static float Bell(float t) => Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));
        static float Ease(float t) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

        /// <summary>동작 정의. 이름은 PlayerMotion·컨트롤러 상태 이름과 같아야 한다.</summary>
        static IEnumerable<ClipSpec> Specs()
        {
            yield return new ClipSpec("Idle", 2f, true, t =>
            {
                float s = Mathf.Sin(2f * Mathf.PI * t);
                return new Pose
                {
                    bob = 0.012f * s, spineLean = 3f + 1.5f * s, headNod = 2f * s, tailSway = 5f * s,
                    right = new Arm(4f, 9f + 2f * s, 14f), left = new Arm(4f, 9f + 2f * s, 14f),
                    rightLeg = new Leg(2f, 4f), leftLeg = new Leg(-2f, 2f),
                };
            });
            // 이동 3.4m/s에 맞춰 한 주기(두 걸음) 0.42초 — 길면 발이 미끄러져 보인다
            yield return new ClipSpec("Run", 0.42f, true, t =>
            {
                float p = 2f * Mathf.PI * t, s = Mathf.Sin(p);
                return new Pose
                {
                    bob = 0.045f * Mathf.Abs(Mathf.Cos(p)) - 0.02f, spineLean = 13f, spineTwist = -7f * s, headNod = -6f, tailSway = 12f * Mathf.Sin(2f * p),
                    right = new Arm(12f - 38f * s, 14f, 78f), left = new Arm(12f + 38f * s, 14f, 78f),
                    rightLeg = new Leg(38f * s, 18f + 62f * Mathf.Max(0f, -Mathf.Cos(p - 0.6f))),
                    leftLeg = new Leg(-38f * s, 18f + 62f * Mathf.Max(0f, Mathf.Cos(p - 0.6f))),
                };
            });
            yield return new ClipSpec("Dodge", 0.22f, false, t =>
            {
                float b = Bell(t);
                return new Pose
                {
                    grounded = false, bob = -0.24f * b, hipLean = 10f * b, spineLean = 22f * b, headNod = 8f * b, tailSway = -15f * b,
                    right = new Arm(4f + 70f * b, 10f + 20f * b, 14f + 70f * b), left = new Arm(4f + 70f * b, 10f + 20f * b, 14f + 70f * b),
                    rightLeg = new Leg(70f * b, 110f * b), leftLeg = new Leg(35f * b, 95f * b),
                };
            });
            yield return new ClipSpec("Jump", 0.65f, false, t =>
            {
                float rise = Ease(t / 0.35f), tuck = Bell(t);
                return new Pose
                {
                    grounded = false, spineLean = -4f + 10f * Ease((t - 0.5f) / 0.5f), headNod = -8f * rise, tailSway = 18f * (1f - t),
                    right = new Arm(4f + 120f * rise - 60f * Ease((t - 0.55f) / 0.45f), 22f, 20f), left = new Arm(4f + 120f * rise - 60f * Ease((t - 0.55f) / 0.45f), 22f, 20f),
                    rightLeg = new Leg(55f * tuck, 90f * tuck), leftLeg = new Leg(12f * tuck, 45f * tuck),
                };
            });
            yield return new ClipSpec("Land", 0.16f, false, t =>
            {
                float c = 1f - Ease(t);
                return new Pose
                {
                    bob = -0.03f * c, spineLean = 3f + 16f * c, headNod = 6f * c,
                    right = new Arm(4f + 22f * c, 10f + 16f * c, 14f + 20f * c), left = new Arm(4f + 22f * c, 10f + 16f * c, 14f + 20f * c),
                    rightLeg = new Leg(38f * c, 70f * c), leftLeg = new Leg(30f * c, 62f * c),
                };
            });
            // 쳐내기: 오른손으로 왼쪽 탄은 오른쪽 어깨 뒤로(ParryLeft), 오른쪽 탄은 왼쪽 어깨 뒤로(ParryRight) 흘린다.
            yield return new ClipSpec("ParryLeft", 0.4f, false, t => Parry(t, fromLeft: true));
            yield return new ClipSpec("ParryRight", 0.4f, false, t => Parry(t, fromLeft: false));
            yield return new ClipSpec("Brace", 1f, true, t =>
            {
                float tremble = 1.2f * Mathf.Sin(2f * Mathf.PI * t * 4f);
                return new Pose
                {
                    bob = -0.02f, hipLean = 6f, spineLean = 20f + tremble, headNod = -4f,
                    right = new Arm(72f, -14f, 75f), left = new Arm(72f, -14f, 75f),
                    rightLeg = new Leg(42f, 78f, 12f), leftLeg = new Leg(30f, 66f, 12f),
                };
            });
            yield return new ClipSpec("Hit", 0.28f, false, t =>
            {
                float b = Bell(t / 0.8f);
                return new Pose
                {
                    spineLean = -22f * b, headNod = -18f * b, sideLean = 6f * b, tailSway = 20f * b,
                    right = new Arm(4f - 12f * b, 10f + 32f * b, 14f + 25f * b), left = new Arm(4f - 12f * b, 10f + 28f * b, 14f + 25f * b),
                    rightLeg = new Leg(14f * b, 20f * b), leftLeg = new Leg(-8f * b, 6f * b),
                };
            });
            yield return new ClipSpec("Fall", 0.65f, false, t =>
            {
                float k = Ease(t / 0.7f), slump = Ease((t - 0.45f) / 0.55f);
                return new Pose
                {
                    bob = -0.05f * k, hipLean = 8f * k, spineLean = -10f * k + 45f * slump, headNod = 35f * slump, tailSway = -10f * k,
                    right = new Arm(4f + 18f * k, 10f + 14f * k, 14f + 10f * k), left = new Arm(4f + 18f * k, 10f + 14f * k, 14f + 10f * k),
                    rightLeg = new Leg(85f * k, 150f * k), leftLeg = new Leg(80f * k, 150f * k),
                };
            });
        }

        static IEnumerable<ClipSpec> PresentationSpecs()
        {
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4f,dx=Mathf.Sin(angle),dz=Mathf.Cos(angle);
                yield return new ClipSpec("Strafe"+i,.5f,true,t=>
                {
                    float wave=Mathf.Sin(t*Mathf.PI*2);
                    return new Pose {bob=.025f*Mathf.Abs(Mathf.Cos(t*Mathf.PI*2)),spineLean=6*dz,sideLean=-5*dx,
                        right=new Arm(16-15*wave*dz,12,40),left=new Arm(16+15*wave*dz,12,40),
                        rightLeg=new Leg(30*wave*dz,18+22*Mathf.Max(0,-wave),3+25*wave*dx),
                        leftLeg=new Leg(-30*wave*dz,18+22*Mathf.Max(0,wave),3+25*wave*dx)};
                });
            }
            yield return new ClipSpec("Strike",.7f,false,t=>
            {
                float prepare=Ease(t/.28f),punch=Ease((t-.28f)/.18f),recover=Ease((t-.55f)/.45f);
                float force=punch*(1-recover);
                return new Pose {spineLean=4+15*force,spineTwist=-18*prepare+40*force,headNod=-5*force,
                    right=new Arm(35*prepare+65*force,12-15*force,85*prepare-70*force),left=new Arm(35*prepare,12,70*prepare),
                    rightLeg=new Leg(18*force,28*force),leftLeg=new Leg(-12*force,12*force)};
            });
            yield return new ClipSpec("Cheer",1.4f,false,t=>new Pose {right=new Arm(140*Bell(t),18,25),left=new Arm(115*Bell(t),22,25),headNod=-7*Bell(t),bob=.035f*Bell(t)});
            yield return new ClipSpec("Play",1.4f,false,t=>{float hop=Mathf.Abs(Mathf.Sin(t*Mathf.PI*2));return new Pose{grounded=false,bob=.16f*hop,right=new Arm(30*hop,24,30),left=new Arm(30*hop,24,30),rightLeg=new Leg(18*hop,30*hop),leftLeg=new Leg(18*hop,30*hop)};});
            yield return new ClipSpec("RunTogether",1.4f,false,t=>{float wave=Mathf.Sin(t*Mathf.PI*6)*Bell(t);return new Pose{spineLean=8*Bell(t),right=new Arm(12-30*wave,12,60),left=new Arm(12+30*wave,12,60),rightLeg=new Leg(30*wave,15+30*Mathf.Abs(wave)),leftLeg=new Leg(-30*wave,15+30*Mathf.Abs(wave)),bob=.025f*Mathf.Abs(wave)};});
            yield return new ClipSpec("Stop",.18f,false,t=>new Pose {spineLean=12*(1-Ease(t)),rightLeg=new Leg(18*(1-Ease(t)),26*(1-Ease(t))),leftLeg=new Leg(-12*(1-Ease(t)),10)});
            yield return new ClipSpec("HitStrong",.42f,false,t=>new Pose {spineLean=-35*Bell(t),headNod=-24*Bell(t),bob=-.05f*Bell(t),right=new Arm(-20*Bell(t),18+38*Bell(t),40),left=new Arm(-16*Bell(t),18+35*Bell(t),35),rightLeg=new Leg(28*Bell(t),50*Bell(t)),leftLeg=new Leg(-18*Bell(t),20*Bell(t))});
            yield return new ClipSpec("Victory",1.2f,false,t=>new Pose {right=new Arm(150*Ease(t/.45f),18,15),left=new Arm(10,16,30),headNod=-8*Ease(t/.45f),tailSway=8*Bell(t)});
            yield return new ClipSpec("Spare",1f,true,t=>new Pose {right=new Arm(42,32,32),left=new Arm(42,32,32),headNod=4*Mathf.Sin(t*Mathf.PI*2),bob=.006f*Mathf.Sin(t*Mathf.PI*2)});
            yield return new ClipSpec("Talk",1f,true,t=>new Pose {right=new Arm(30+12*Mathf.Sin(t*Mathf.PI*2),20,45),left=new Arm(8,12,20),headNod=3*Mathf.Sin(t*Mathf.PI*2)});
            yield return new ClipSpec("Nod",.7f,true,t=>new Pose {headNod=18*Bell(t),right=new Arm(6,12,20),left=new Arm(6,12,20)});
            yield return new ClipSpec("Surprise",.65f,false,t=>new Pose {headNod=-12*Bell(t),spineLean=-10*Bell(t),right=new Arm(55*Ease(t/.3f),20,65),left=new Arm(55*Ease(t/.3f),20,65)});
        }

        static Pose Parry(float t, bool fromLeft)
        {
            // 0~0.25 준비(반대쪽으로 끌어옴) → 0.25~0.6 휘두름 → 0.6~1 복귀
            float wind = Ease(t / 0.25f), swing = Ease((t - 0.25f) / 0.35f), back = Ease((t - 0.6f) / 0.4f);
            float startOut = fromLeft ? -40f : 55f, endOut = fromLeft ? 60f : -35f;
            float outward = Mathf.Lerp(Mathf.Lerp(10f, startOut, wind), endOut, swing);
            outward = Mathf.Lerp(outward, 10f, back);
            float raise = Mathf.Lerp(Mathf.Lerp(4f, 70f, wind), 4f, back);
            float twist = (fromLeft ? 1f : -1f) * Mathf.Lerp(Mathf.Lerp(0f, -18f, wind), 22f, swing) * (1f - back);
            return new Pose
            {
                spineLean = 8f, spineTwist = twist, headNod = -3f,
                right = new Arm(raise, outward, Mathf.Lerp(Mathf.Lerp(14f, 40f, wind), 14f, back)),
                left = new Arm(4f, 16f, 30f),
                rightLeg = new Leg(14f, 20f, 8f), leftLeg = new Leg(-10f, 8f, 8f),
            };
        }

        [MenuItem("Tools/Heroine/동작 클립 다시 만들기")]
        public static void Build()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (!controller) throw new InvalidOperationException("컨트롤러 없음: " + ControllerPath);
            if (!AssetDatabase.IsValidFolder(ClipFolder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Characters/Heroine", "Clips");

            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                var rig = new Rig(instance.GetComponentInChildren<Animator>().transform);
                var states = controller.layers[0].stateMachine.states.ToDictionary(s => s.state.name, s => s.state);
                foreach (var spec in Specs().Concat(PresentationSpecs()))
                {
                    var clip = rig.Bake(spec);
                    string path = $"{ClipFolder}/Heroine_{spec.name}.anim";
                    var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (existing) { EditorUtility.CopySerialized(clip, existing); clip = existing; }
                    else AssetDatabase.CreateAsset(clip, path);
                    if (!states.TryGetValue(spec.name, out var state)) state=controller.layers[0].stateMachine.AddState(spec.name);
                    state.motion = clip;
                }
                foreach(var name in new[]{"MoveX","MoveY"})
                    if(!controller.parameters.Any(item=>item.name==name))controller.AddParameter(name,AnimatorControllerParameterType.Float);
                var strafe=controller.layers[0].stateMachine.states.FirstOrDefault(item=>item.state.name=="Strafe").state;
                if(!strafe)strafe=controller.layers[0].stateMachine.AddState("Strafe");
                var tree=AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<BlendTree>().FirstOrDefault(item=>item.name=="록온8방향");
                if(!tree){tree=new BlendTree{name="록온8방향"};AssetDatabase.AddObjectToAsset(tree,controller);}
                tree.blendType=BlendTreeType.FreeformDirectional2D;tree.blendParameter="MoveX";tree.blendParameterY="MoveY";tree.useAutomaticThresholds=false;
                var children=new List<ChildMotion>{new(){motion=AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipFolder+"/Heroine_Idle.anim"),position=Vector2.zero,timeScale=1}};
                for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;children.Add(new ChildMotion{motion=AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipFolder+"/Heroine_Strafe"+i+".anim"),position=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle)),timeScale=1});}
                tree.children=children.ToArray();strafe.motion=tree;EditorUtility.SetDirty(tree);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                Debug.Log($"[HeroineClipAuthoring] 동작 클립 다시 만듦 — 앞 {rig.Forward}, 오른쪽 {rig.Right}");
            }
            finally { Object.DestroyImmediate(instance); }
        }

        public static void BuildFromCommandLine()
        {
            try { Build(); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        /// <summary>모델 골격과 캐릭터 방향. 자세를 실제 뼈 회전으로 바꾼다.</summary>
        sealed class Rig
        {
            readonly Transform root;
            readonly Transform[] bones;
            readonly Quaternion[] bindLocal;
            readonly Vector3 hipsBindLocal;
            readonly Transform hips, spine, head, rightTail, leftTail;
            readonly Transform rUpper, rFore, rHand, lUpper, lFore, lHand;
            readonly Transform rThigh, rShin, rFoot, lThigh, lShin, lFoot;
            readonly float hipsHeight, thighLength, shinLength;
            public Vector3 Forward { get; }
            public Vector3 Right { get; }

            public Rig(Transform animatorRoot)
            {
                root = animatorRoot;
                var all = root.GetComponentsInChildren<Transform>(true);
                Transform F(string n) => all.FirstOrDefault(t => t.name == n) ?? throw new InvalidOperationException("뼈 없음: " + n);
                hips = F("Hips"); spine = F("Spine"); head = F("HeadBone"); rightTail = F("RightTail"); leftTail = F("LeftTail");
                bones = hips.GetComponentsInChildren<Transform>(true);
                bindLocal = bones.Select(b => b.localRotation).ToArray();
                hipsBindLocal = hips.localPosition;

                // 앞 방향: 머리 뼈에서 눈 메시 중심 쪽
                var eye = all.First(t => t.name == "Eye_R_Base").GetComponent<SkinnedMeshRenderer>();
                var baked = new Mesh(); eye.BakeMesh(baked);
                var eyeCenter = eye.transform.TransformPoint(baked.bounds.center);
                Object.DestroyImmediate(baked);
                float forwardSign = Mathf.Sign(eyeCenter.z - head.position.z);
                Forward = new Vector3(0f, 0f, forwardSign);
                Right = Vector3.Cross(Vector3.up, Forward);

                // 해부학적 오른팔·오른다리는 이름이 아니라 실제 위치로 고른다 (glTF 좌우 반전 대비)
                var armA = F("RightUpperArm"); var armB = F("LeftUpperArm");
                bool nameMatches = Vector3.Dot(armA.position - spine.position, Right) > 0f;
                rUpper = nameMatches ? armA : armB; lUpper = nameMatches ? armB : armA;
                rFore = rUpper.GetChild(0); rHand = rFore.GetChild(0); lFore = lUpper.GetChild(0); lHand = lFore.GetChild(0);
                var legA = F("RightUpperLeg"); var legB = F("LeftUpperLeg");
                bool legMatches = Vector3.Dot(legA.position - hips.position, Right) > 0f;
                rThigh = legMatches ? legA : legB; lThigh = legMatches ? legB : legA;
                rShin = rThigh.GetChild(0); rFoot = rShin.GetChild(0); lShin = lThigh.GetChild(0); lFoot = lShin.GetChild(0);

                thighLength = Vector3.Distance(rThigh.position, rShin.position);
                shinLength = Vector3.Distance(rShin.position, rFoot.position);
                hipsHeight = hips.position.y - root.position.y;
            }

            void ResetPose()
            {
                for (int i = 0; i < bones.Length; i++) bones[i].localRotation = bindLocal[i];
                hips.localPosition = hipsBindLocal;
            }

            Vector3 ArmDirection(float side, Arm a, float extraForward)
            {
                float f = (a.forward + extraForward) * Mathf.Deg2Rad, o = a.outward * Mathf.Deg2Rad;
                return (Right * side * Mathf.Sin(o) - Vector3.up * Mathf.Cos(o) * Mathf.Cos(f) + Forward * Mathf.Cos(o) * Mathf.Sin(f)).normalized;
            }

            Vector3 LegDirection(float side, float forward, float outward)
            {
                float f = forward * Mathf.Deg2Rad, o = outward * Mathf.Deg2Rad;
                return (Right * side * Mathf.Sin(o) - Vector3.up * Mathf.Cos(o) * Mathf.Cos(f) + Forward * Mathf.Cos(o) * Mathf.Sin(f)).normalized;
            }

            static void Aim(Transform bone, Transform child, Vector3 direction)
            {
                var current = child.position - bone.position;
                if (current.sqrMagnitude < 1e-8f) return;
                bone.rotation = Quaternion.FromToRotation(current, direction) * bone.rotation;
            }

            void Apply(Pose p)
            {
                ResetPose();
                var footRestR = rFoot.rotation; var footRestL = lFoot.rotation;

                // 다리 굽힘으로 엉덩이가 내려가는 양 (땅에 선 동작만)
                float Reach(Leg l) => thighLength * Mathf.Cos(l.forward * Mathf.Deg2Rad) + shinLength * Mathf.Cos((l.forward - l.knee) * Mathf.Deg2Rad);
                float drop = p.grounded ? (thighLength + shinLength) - Mathf.Max(Reach(p.rightLeg), Reach(p.leftLeg)) : 0f;
                hips.position += Vector3.up * (p.bob * hipsHeight - Mathf.Max(0f, drop));

                var hipRotation = Quaternion.AngleAxis(p.hipLean, Right);
                hips.rotation = hipRotation * hips.rotation;
                var body = Quaternion.AngleAxis(p.spineTwist, Vector3.up) * Quaternion.AngleAxis(p.sideLean, Forward) * Quaternion.AngleAxis(p.spineLean, Right);
                spine.rotation = body * spine.rotation;
                var upperBody = body * hipRotation;
                head.rotation = Quaternion.AngleAxis(p.headNod, upperBody * Right) * head.rotation;
                rightTail.rotation = Quaternion.AngleAxis(p.tailSway, upperBody * Forward) * rightTail.rotation;
                leftTail.rotation = Quaternion.AngleAxis(p.tailSway, upperBody * Forward) * leftTail.rotation;

                // 팔은 상체 기준, 다리는 캐릭터 기준
                Aim(rUpper, rFore, upperBody * ArmDirection(1f, p.right, 0f));
                Aim(rFore, rHand, upperBody * ArmDirection(1f, p.right, p.right.elbow));
                Aim(lUpper, lFore, upperBody * ArmDirection(-1f, p.left, 0f));
                Aim(lFore, lHand, upperBody * ArmDirection(-1f, p.left, p.left.elbow));

                Aim(rThigh, rShin, LegDirection(1f, p.rightLeg.forward, p.rightLeg.outward));
                Aim(rShin, rFoot, LegDirection(1f, p.rightLeg.forward - p.rightLeg.knee, p.rightLeg.outward));
                Aim(lThigh, lShin, LegDirection(-1f, p.leftLeg.forward, p.leftLeg.outward));
                Aim(lShin, lFoot, LegDirection(-1f, p.leftLeg.forward - p.leftLeg.knee, p.leftLeg.outward));
                if (p.grounded) { rFoot.rotation = footRestR; lFoot.rotation = footRestL; }
            }

            public AnimationClip Bake(ClipSpec spec)
            {
                int frames = Mathf.Max(2, Mathf.RoundToInt(spec.length * FrameRate) + 1);
                var paths = bones.Select(b => AnimationUtility.CalculateTransformPath(b, root)).ToArray();
                var rotationKeys = bones.Select(_ => new List<Keyframe>[4] { new(), new(), new(), new() }).ToArray();
                var positionKeys = new List<Keyframe>[3] { new(), new(), new() };
                var previous = new Quaternion[bones.Length];

                for (int f = 0; f < frames; f++)
                {
                    float normalized = f / (float)(frames - 1);
                    float time = normalized * spec.length;
                    Apply(spec.pose(spec.loop && f == frames - 1 ? 0f : normalized));
                    for (int i = 0; i < bones.Length; i++)
                    {
                        var q = bones[i].localRotation;
                        if (f > 0 && Quaternion.Dot(q, previous[i]) < 0f) q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
                        previous[i] = q;
                        rotationKeys[i][0].Add(new Keyframe(time, q.x)); rotationKeys[i][1].Add(new Keyframe(time, q.y));
                        rotationKeys[i][2].Add(new Keyframe(time, q.z)); rotationKeys[i][3].Add(new Keyframe(time, q.w));
                    }
                    var hp = hips.localPosition;
                    positionKeys[0].Add(new Keyframe(time, hp.x)); positionKeys[1].Add(new Keyframe(time, hp.y)); positionKeys[2].Add(new Keyframe(time, hp.z));
                }
                ResetPose();

                var clip = new AnimationClip { name = "Heroine_" + spec.name, frameRate = FrameRate };
                string[] rotationProps = { "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w" };
                for (int i = 0; i < bones.Length; i++)
                    for (int c = 0; c < 4; c++)
                        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(paths[i], typeof(Transform), rotationProps[c]), Smooth(rotationKeys[i][c]));
                string hipsPath = AnimationUtility.CalculateTransformPath(hips, root);
                string[] positionProps = { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z" };
                for (int c = 0; c < 3; c++)
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(hipsPath, typeof(Transform), positionProps[c]), Smooth(positionKeys[c]));
                clip.EnsureQuaternionContinuity();
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = spec.loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                return clip;
            }

            static AnimationCurve Smooth(List<Keyframe> keys)
            {
                var curve = new AnimationCurve(keys.ToArray());
                for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
                return curve;
            }
        }
    }
}
