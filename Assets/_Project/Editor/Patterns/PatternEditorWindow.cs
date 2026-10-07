using Game2Week.Data.Patterns;
using Game2Week.Battle.Patterns.Trajectories;
using UnityEditor;
using UnityEngine;

namespace Game2Week.EditorTools.Patterns
{
    public sealed class PatternEditorWindow : EditorWindow
    {
        GraphPatternDefinition[] list=System.Array.Empty<GraphPatternDefinition>();
        GraphPatternDefinition selected,draft;
        SerializedObject properties;
        PatternPreview preview;
        SequenceEditorPanel sequencePanel;
        int tab;
        Vector2 scroll,leftScroll;
        float time,yaw=20,pitch=30,duration=12;
        Vector2 arena=new Vector2(8,10);
        Vector3 enemy=new Vector3(0,0,3),target=new Vector3(0,0,-3);
        bool playing,loop=true,waist,dirty;
        double lastUpdate;
        string notice="";
        [MenuItem("Tools/Pattern Editor")]
        public static void Open()=>GetWindow<PatternEditorWindow>("Pattern Editor");
        void OnEnable(){minSize=new Vector2(1000,650);Refresh();preview=new PatternPreview();sequencePanel=new SequenceEditorPanel();lastUpdate=EditorApplication.timeSinceStartup;EditorApplication.update+=UpdatePreview;}
        void OnDisable(){EditorApplication.update-=UpdatePreview;preview?.Dispose();sequencePanel?.Dispose();if(draft)DestroyImmediate(draft);}
        void Refresh()=>list=PatternAssetStore.List();
        void Select(GraphPatternDefinition asset)
        {
            if(dirty&&!EditorUtility.DisplayDialog("저장 전 변경", "변경을 버리고 다른 패턴을 열까요?", "버리기", "취소"))return;
            if(draft)DestroyImmediate(draft);selected=asset;
            draft=Instantiate(asset);draft.name=asset.name;draft.hideFlags=HideFlags.HideAndDontSave;properties=new SerializedObject(draft);
            dirty=false;playing=false;time=0;notice="";
        }
        void UpdatePreview()
        {
            double now=EditorApplication.timeSinceStartup;
            if(playing){time+=Mathf.Min(.1f,(float)(now-lastUpdate));if(time>=duration){time=loop?time%duration:duration;if(!loop)playing=false;}Repaint();}
            lastUpdate=now;
        }
        void OnGUI()
        {
            tab=GUILayout.Toolbar(tab,new[]{"그래프 패턴","시퀀스"});
            if(tab==1){sequencePanel.Draw();return;}
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(205));
            EditorGUILayout.LabelField("패턴",EditorStyles.boldLabel);
            if(GUILayout.Button("새 패턴"))Create(null);
            using(new EditorGUI.DisabledScope(!selected))
            {
                if(GUILayout.Button("복제"))Create(draft);
                if(GUILayout.Button("삭제")&&EditorUtility.DisplayDialog("패턴 삭제","정의·프리팹·공격 에셋을 삭제합니다. 맵에서 사용하는 패턴은 삭제할 수 없습니다.","삭제","취소"))
                {try{PatternAssetStore.Delete(selected);selected=null;DestroyImmediate(draft);draft=null;dirty=false;Refresh();}catch(System.Exception e){notice=e.Message;}}
            }
            if(GUILayout.Button("8종 프리셋 추가")){PatternAssetStore.CreatePresets();Refresh();}
            leftScroll=EditorGUILayout.BeginScrollView(leftScroll);
            foreach(var asset in list)if(GUILayout.Toggle(selected==asset,asset.name,"Button")&&selected!=asset)Select(asset);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.HelpBox("저장하면 Attack_이름이 카탈로그에 등록됩니다. Stage Editor의 패턴 목록에서 연결하세요. 기존 맵은 자동 변경하지 않습니다.",MessageType.Info);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            if(!draft){EditorGUILayout.HelpBox("패턴을 선택하거나 새로 만드세요.",MessageType.Info);EditorGUILayout.EndVertical();EditorGUILayout.EndHorizontal();return;}
            var errors=PatternGraphRules.Validate(draft);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button(playing?"일시정지":"재생",GUILayout.Width(80)))playing=!playing;
            if(GUILayout.Button("정지",GUILayout.Width(55))){playing=false;time=0;}
            if(GUILayout.Button("한 프레임",GUILayout.Width(90))){playing=false;time=Mathf.Min(duration,time+1/60f);}
            loop=GUILayout.Toggle(loop,"반복");waist=GUILayout.Toggle(waist,"허리 시점");
            duration=EditorGUILayout.Slider("길이(초)",duration,1,30);
            EditorGUILayout.EndHorizontal();
            time=EditorGUILayout.Slider("시간",time,0,duration);
            var rect=GUILayoutUtility.GetRect(300,Mathf.Max(230,position.height*.42f),GUILayout.ExpandWidth(true));
            if(errors.Count==0&&Event.current.type==EventType.Repaint)preview.Draw(rect,draft,time,yaw,pitch,waist,arena,enemy,target);
            yaw=EditorGUILayout.Slider("카메라 회전",yaw,-180,180);pitch=EditorGUILayout.Slider("카메라 높이",pitch,5,75);
            EditorGUILayout.BeginHorizontal();arena=EditorGUILayout.Vector2Field("경기장(m)",arena);enemy=EditorGUILayout.Vector3Field("발사 적",enemy);target=EditorGUILayout.Vector3Field("목표",target);EditorGUILayout.EndHorizontal();
            arena=new Vector2(Mathf.Clamp(arena.x,2,30),Mathf.Clamp(arena.y,2,30));
            EditorGUILayout.LabelField($"활성 탄 {preview.ActiveCount} · 시드 {draft.seed} · 변경 {(dirty?"저장 전":"없음")}");
            if(preview.HasBoundaryCrossing)EditorGUILayout.HelpBox("예고 궤적 일부가 경계를 넘습니다. 실제 게임에서는 경계 밖 탄을 회수합니다.",MessageType.Warning);
            scroll=EditorGUILayout.BeginScrollView(scroll);
            properties.Update();EditorGUI.BeginChangeCheck();
            Property("trajectory","궤적");Property("color","대응 색");Property("origin","발사원");Property("layout","발사 배열");Property("shotCount","발사 수");Property("spreadDegrees","부채꼴 각도");
            Property("seed","고정 시드");Property("aimRadius","조준 오차(m)");Property("warningSeconds","예고(초)");Property("lifetime","탄 수명(초)");Property("height","탄 높이(m)");Property("speed","기준 속도(m/s)");
            Property("amplitude","진폭/높이(m)");Property("frequency","주파수(진행 m당 주기)");Property("arcRadius","원호 반경(m)");
            if(draft.trajectory==TrajectoryKind.Bezier){Property("bezierControl1","제어점 1");Property("bezierControl2","제어점 2");Property("bezierEnd","끝점");}
            Property("minimumInterval","최소 발사 간격(초)");Property("interval","발사 간격: X 패턴 시간(0~30초), Y 초");Property("speedMultiplier","속도 배율: X 수명 비율(0~1), Y 배율(0~5)");
            if(EditorGUI.EndChangeCheck()){properties.ApplyModifiedProperties();dirty=true;}
            errors=PatternGraphRules.Validate(draft);foreach(var error in errors)EditorGUILayout.HelpBox(error,MessageType.Error);
            EditorGUILayout.HelpBox("주파수는 이동 거리 기준입니다. 속도 그래프는 경로 진행률을 조절하며 굽은 경로의 실제 이동 속도와는 차이가 납니다. 노랑만 쳐내기 가능하며 빨강/파랑 판정은 Claude 4단계와 합친 뒤 적용됩니다.",MessageType.Info);
            using(new EditorGUI.DisabledScope(errors.Count>0))if(GUILayout.Button("저장 · 카탈로그 등록"))
            {
                try{EditorUtility.CopySerialized(draft,selected);selected.hideFlags=HideFlags.None;PatternAssetStore.Save(selected);dirty=false;notice="저장 완료 — Stage Editor에서 Attack_"+selected.name+" 선택";}catch(System.Exception e){notice=e.Message;}
            }
            if(!string.IsNullOrEmpty(notice))EditorGUILayout.HelpBox(notice,MessageType.Info);
            EditorGUILayout.EndScrollView();EditorGUILayout.EndVertical();EditorGUILayout.EndHorizontal();
        }
        void Property(string name,string label)=>EditorGUILayout.PropertyField(properties.FindProperty(name),new GUIContent(label),true);
        void Create(GraphPatternDefinition source)
        {
            try{var asset=PatternAssetStore.Create(source?source.name+"_Copy":"NewPattern",source);Refresh();Select(asset);}catch(System.Exception e){notice=e.Message;}
        }
    }
}
