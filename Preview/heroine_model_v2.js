import * as THREE from './vendor/three/build/three.module.js';
export function createHeroine(){
const COLORS = {
  skin: '#ffe4d6', hair: '#f6d466', tie: '#5f8bcf',
  hoodie: '#a3c4ee', hoodieDark: '#8eb2e2',
  jeans: '#5f86c0', jeansDark: '#4f74ab',
  shoe: '#f3f4f6', sole: '#d6d9df', lace: '#c9ccd3',
  eyeDark: '#3b2a22', iris: '#f5c63a', irisDeep: '#d9971c', white: '#ffffff',
  blush: '#f3b2a8', mouth: '#7a4038',
};
const materials = {};
function mat(name) {
  if (!materials[name]) {
    materials[name] = new THREE.MeshStandardMaterial({
      name, color: COLORS[name], roughness: 0.9, metalness: 0, flatShading: true,
      side: name === 'hair' ? THREE.DoubleSide : THREE.FrontSide,
    });
  }
  return materials[name];
}

// ---------- 지오메트리 헬퍼 ----------
const V = (x, y, z) => new THREE.Vector3(x, y, z);

function hash(x, y, z) {
  const s = Math.sin(x * 127.1 + y * 311.7 + z * 74.7) * 43758.5453;
  return s - Math.floor(s);
}
// 같은 위치의 정점은 같은 오프셋을 받으므로 이음매가 벌어지지 않는다
function jitter(geo, amount) {
  const p = geo.attributes.position;
  const v = new THREE.Vector3();
  for (let i = 0; i < p.count; i++) {
    v.fromBufferAttribute(p, i);
    const kx = Math.round(v.x * 1e4), ky = Math.round(v.y * 1e4), kz = Math.round(v.z * 1e4);
    p.setXYZ(i,
      v.x + (hash(kx, ky, kz) - 0.5) * amount,
      v.y + (hash(ky, kz, kx) - 0.5) * amount,
      v.z + (hash(kz, kx, ky) - 0.5) * amount);
  }
  return geo;
}

// 곡선을 따라 반지름이 변하는 튜브 (팔, 다리, 머리카락)
// side를 주면 단면의 ellipse[1] 축이 그 방향을 따른다 (납작한 머리카락 덩어리용)
function tube(points, radiusFn, { segments = 10, radial = 7, ellipse = [1, 1], side = null } = {}) {
  const curve = new THREE.CatmullRomCurve3(points);
  const frames = curve.computeFrenetFrames(segments, false);
  const pos = [], idx = [];
  for (let i = 0; i <= segments; i++) {
    const t = i / segments, P = curve.getPointAt(t), r = radiusFn(t);
    let N = frames.normals[i], B = frames.binormals[i];
    if (side) {
      const T = frames.tangents[i];
      B = side.clone().addScaledVector(T, -side.dot(T)).normalize();
      N = B.clone().cross(T);
    }
    for (let j = 0; j < radial; j++) {
      const a = j / radial * Math.PI * 2, c = Math.cos(a) * ellipse[0] * r, s = Math.sin(a) * ellipse[1] * r;
      pos.push(P.x + N.x * c + B.x * s, P.y + N.y * c + B.y * s, P.z + N.z * c + B.z * s);
    }
  }
  for (let i = 0; i < segments; i++) {
    for (let j = 0; j < radial; j++) {
      const a = i * radial + j, b = i * radial + (j + 1) % radial;
      const c = (i + 1) * radial + j, d = (i + 1) * radial + (j + 1) % radial;
      idx.push(a, b, c, b, d, c);
    }
  }
  const start = pos.length / 3, P0 = curve.getPointAt(0);
  pos.push(P0.x, P0.y, P0.z);
  const end = start + 1, P1 = curve.getPointAt(1);
  pos.push(P1.x, P1.y, P1.z);
  for (let j = 0; j < radial; j++) {
    const jn = (j + 1) % radial;
    idx.push(start, jn, j);
    idx.push(end, segments * radial + j, segments * radial + jn);
  }
  const geo = new THREE.BufferGeometry();
  geo.setAttribute('position', new THREE.Float32BufferAttribute(pos, 3));
  geo.setIndex(idx);
  return geo;
}

const ellipseGeo = (rx, ry, seg = 12) => new THREE.CircleGeometry(1, seg).scale(rx, ry, 1);

let triangleCount = 0;
function part(name, geo, matName, parent, position) {
  const flat = geo.index ? geo.toNonIndexed() : geo;
  flat.computeVertexNormals();
  triangleCount += flat.attributes.position.count / 3;
  const mesh = new THREE.Mesh(flat, mat(matName));
  mesh.name = name;
  mesh.castShadow = true;
  if (position) mesh.position.copy(position);
  parent.add(mesh);
  return mesh;
}

// ---------- 캐릭터 (키 ≈ 1.0, +Z 방향을 바라봄) ----------
const character = new THREE.Group();
character.name = 'Heroine';

// --- 머리 ---
const head = new THREE.Group();
head.name = 'Head';
head.position.set(0, 0.765, 0);
character.add(head);

const FACE = { rx: 0.165, ry: 0.16, rz: 0.157 };
const jaw = y => (y < -0.07 ? 1 - 0.35 * Math.min(1, (-y - 0.07) / 0.09) : 1);
{
  const g = new THREE.SphereGeometry(1, 20, 16).scale(FACE.rx, FACE.ry, FACE.rz);
  const p = g.attributes.position;
  for (let i = 0; i < p.count; i++) p.setX(i, p.getX(i) * jaw(p.getY(i)));
  part('Face', g, 'skin', head);
}
function faceZ(x,y) {
  const face=head.children.find(p=>p.name==='Face');
  const surface=new THREE.Mesh(face.geometry,face.material);
  const ray=new THREE.Raycaster(new THREE.Vector3(x,y,1),new THREE.Vector3(0,0,-1));
  const hit=ray.intersectObject(surface,false)[0];return hit?hit.point.z:0;
}
// 얼굴 곡면에 붙이는 평면 파츠 (눈, 눈썹, 입, 볼터치)
function decal(name, geo, cx, cy, layer, matName) {
  geo.translate(cx, cy, 0);
  const p = geo.attributes.position;
  for (let i = 0; i < p.count; i++) p.setZ(i, faceZ(p.getX(i), p.getY(i)) + 0.0005 + layer * 0.00025);
  return part(name, geo, matName, head);
}

for (const s of [-1, 1]) {
  const side = s < 0 ? 'R' : 'L';
  const ex = 0.068 * s, ey = -0.075;
  decal(`Eye_${side}_Base`, ellipseGeo(0.037, 0.043), ex, ey, 0, 'eyeDark');
  decal(`Eye_${side}_Iris`, ellipseGeo(0.031, 0.036), ex, ey - 0.005, 1, 'iris');
  decal(`Eye_${side}_IrisDeep`, ellipseGeo(0.022, 0.025), ex, ey - 0.009, 2, 'irisDeep');
  decal(`Eye_${side}_Pupil`, ellipseGeo(0.010, 0.014), ex, ey - 0.007, 3, 'eyeDark');
  decal(`Eye_${side}_Shine`, ellipseGeo(0.009, 0.011, 8), ex - 0.011, ey + 0.011, 4, 'white');
  decal(`Eye_${side}_Shine2`, ellipseGeo(0.004, 0.005, 6), ex + 0.011, ey - 0.02, 4, 'white');
  decal(`Eye_${side}_Lash`, new THREE.RingGeometry(0.037, 0.046, 12, 1, Math.PI * 0.06, Math.PI * 0.88).scale(1, 1.1, 1), ex, ey - 0.001, 1, 'eyeDark');

  // 걱정스러운 눈썹: 안쪽 끝이 더 높다
  const brow = new THREE.Shape([
    new THREE.Vector2(0.036 * s, -0.016), new THREE.Vector2(0.098 * s, -0.029),
    new THREE.Vector2(0.098 * s, -0.035), new THREE.Vector2(0.036 * s, -0.023),
  ]);
  decal(`Brow_${side}`, new THREE.ShapeGeometry(brow), 0, 0, 0, 'eyeDark');
  decal(`Blush_${side}`, ellipseGeo(0.024, 0.012, 10), 0.105 * s, -0.113, 0, 'blush');

  // 귀
  part(`Ear_${side}`, new THREE.SphereGeometry(1, 6, 5).scale(0.016, 0.03, 0.024), 'skin', head, V(0.162 * s, -0.045, -0.01));
}
// 작은 시무룩한 입
decal('Mouth', new THREE.RingGeometry(0.011, 0.0155, 6, 1, Math.PI * 0.22, Math.PI * 0.56), 0, -0.14, 0, 'mouth');

// --- 머리카락 ---
{
  const top = new THREE.SphereGeometry(0.185, 14, 9, 0, Math.PI * 2, 0, Math.PI * 0.56).scale(1.04, 1, 1.03).translate(0, 0.02, -0.012);
  part('Hair_Top', jitter(top, 0.008), 'hair', head);
  const back = new THREE.SphereGeometry(0.19, 14, 9, Math.PI * 1.05, Math.PI * 0.9, Math.PI * 0.45, Math.PI * 0.42).scale(1.03, 1, 1.04).translate(0, 0.015, -0.015);
  part('Hair_Back', jitter(back, 0.008), 'hair', head);
}
const hairZ = (x, y) => -0.012 + 1.03 * Math.sqrt(Math.max(0, 0.185 ** 2 - (x / 1.04) ** 2 - (y - 0.02) ** 2));
// 앞머리: [x, 끝 높이] — 가로로 넓고 납작한 덩어리
const bangs = [[-0.125, -0.05], [-0.07, -0.025], [-0.018, -0.04], [0.035, -0.022], [0.088, -0.03], [0.13, -0.055]];
bangs.forEach(([x, yEnd], i) => {
  const g = tube([
    V(x * 0.8, 0.13, hairZ(x * 0.8, 0.13) - 0.006),
    V(x, 0.04, hairZ(x, 0.04) + 0.004),
    V(x * 1.06, yEnd, faceZ(x * 1.06, yEnd) + 0.01),
  ], t => 0.034 * (1 - t) + 0.005, { segments: 6, radial: 6, ellipse: [0.45, 1.5], side: V(1, 0, 0) });
  part(`Hair_Bang_${i}`, jitter(g, 0.004), 'hair', head);
});
for (const s of [-1, 1]) {
  const side = s < 0 ? 'R' : 'L';
  // 옆머리
  const lock = tube([V(0.15 * s, 0.06, 0.07), V(0.168 * s, -0.03, 0.075), V(0.163 * s, -0.12, 0.065), V(0.155 * s, -0.155, 0.06)],
    t => 0.028 * (1 - t) + 0.005, { segments: 7, radial: 6, ellipse: [1.3, 0.6], side: V(1, 0, 0) });
  part(`Hair_SideLock_${side}`, jitter(lock, 0.004), 'hair', head);

  // 양갈래
  const tail = tube([
    V(0.15 * s, 0.10, -0.085), V(0.22 * s, 0.085, -0.10), V(0.28 * s, 0.0, -0.10),
    V(0.295 * s, -0.12, -0.09), V(0.28 * s, -0.22, -0.08), V(0.305 * s, -0.28, -0.07),
  ], t => (0.028 + 0.05 * Math.sin(Math.PI * Math.pow(t, 0.8))) * (1 - 0.85 * t * t * t), { segments: 14, radial: 7, ellipse: [1, 0.8], side: V(0, 0, 1) });
  part(`Hair_Tail_${side}`, jitter(tail, 0.007), 'hair', head);

  const tie = new THREE.TorusGeometry(0.03, 0.013, 5, 8).rotateY(Math.PI / 2);
  part(`Hair_Tie_${side}`, tie, 'tie', head, V(0.168 * s, 0.098, -0.087));
}

// --- 몸통 (후드티) ---
const lathe = (pts, seg = 8) => new THREE.LatheGeometry(pts.map(([r, y]) => new THREE.Vector2(r, y)), seg, Math.PI / 8);
part('Hoodie_Body', lathe([[0, 0.325], [0.118, 0.325], [0.124, 0.36], [0.118, 0.42], [0.122, 0.47], [0.112, 0.515], [0.085, 0.548], [0.045, 0.565], [0, 0.57]]).scale(1, 1, 0.78), 'hoodie', character);
part('Hoodie_Hem', lathe([[0.112, 0.298], [0.126, 0.304], [0.128, 0.33], [0.12, 0.336]]).scale(1, 1, 0.8), 'hoodieDark', character);
part('Hoodie_Collar', new THREE.TorusGeometry(0.055, 0.03, 6, 10).rotateX(Math.PI / 2).scale(1.05, 1, 0.85), 'hoodie', character, V(0, 0.567, -0.005));
part('Hoodie_Hood', new THREE.SphereGeometry(1, 8, 6).scale(0.095, 0.075, 0.05), 'hoodie', character, V(0, 0.53, -0.085));
{
  const pocket = new THREE.Shape([
    new THREE.Vector2(-0.066, 0.345), new THREE.Vector2(0.066, 0.345),
    new THREE.Vector2(0.05, 0.415), new THREE.Vector2(-0.05, 0.415),
  ]);
  const g = new THREE.ExtrudeGeometry(pocket, { depth: 0.009, bevelEnabled: false }).translate(0, 0, 0.083);
  part('Hoodie_Pocket', g, 'hoodieDark', character);
}
part('Neck', new THREE.CylinderGeometry(0.034, 0.038, 0.08, 7), 'skin', character, V(0, 0.6, 0));

// --- 팔 ---
for (const s of [-1, 1]) {
  const side = s < 0 ? 'R' : 'L';
  part(`Shoulder_${side}`, new THREE.SphereGeometry(0.05, 8, 6), 'hoodie', character, V(0.095 * s, 0.505, 0));
  const sleeve = tube([V(0.085 * s, 0.52, 0), V(0.14 * s, 0.49, 0), V(0.20 * s, 0.44, 0.005), V(0.245 * s, 0.405, 0.01)],
    t => 0.042 + 0.012 * Math.sin(Math.PI * t) - 0.008 * t, { segments: 8, radial: 8 });
  part(`Sleeve_${side}`, sleeve, 'hoodie', character);
  part(`Cuff_${side}`, tube([V(0.238 * s, 0.409, 0.01), V(0.268 * s, 0.39, 0.012)], () => 0.031, { segments: 1, radial: 8 }), 'hoodieDark', character);
  const hand = part(`Hand_${side}`, new THREE.SphereGeometry(1, 7, 5).scale(0.038, 0.024, 0.032), 'skin', character, V(0.293 * s, 0.372, 0.012));
  hand.rotation.z = -0.66 * s;
  part(`Thumb_${side}`, new THREE.SphereGeometry(1, 5, 4).scale(0.009, 0.013, 0.009), 'skin', character, V(0.279 * s, 0.375, 0.032));
}

// --- 다리 (청바지) ---
part('Jeans_Hip', new THREE.SphereGeometry(1, 10, 6).scale(0.115, 0.06, 0.085), 'jeans', character, V(0, 0.33, 0));
for (const s of [-1, 1]) {
  const side = s < 0 ? 'R' : 'L';
  part(`Leg_${side}`, tube([V(0.058 * s, 0.34, 0), V(0.062 * s, 0.2, 0), V(0.066 * s, 0.07, 0)], t => 0.047 - 0.004 * t, { segments: 6, radial: 8 }), 'jeans', character);
  part(`Jeans_Cuff_${side}`, tube([V(0.066 * s, 0.098, 0), V(0.066 * s, 0.066, 0)], () => 0.05, { segments: 1, radial: 8 }), 'jeansDark', character);

  // --- 신발 ---
  const shoe = new THREE.Group();
  shoe.name = `Shoe_${side}`;
  shoe.position.set(0.068 * s, 0, 0.015);
  character.add(shoe);
  part(`Shoe_${side}_Sole`, new THREE.BoxGeometry(0.092, 0.02, 0.15), 'sole', shoe, V(0, 0.01, 0));
  part(`Shoe_${side}_Upper`, new THREE.SphereGeometry(1, 10, 5, 0, Math.PI * 2, 0, Math.PI / 2).scale(0.045, 0.07, 0.076), 'shoe', shoe, V(0, 0.02, 0));
  for (let k = 0; k < 2; k++) {
    const lace = part(`Shoe_${side}_Lace${k}`, new THREE.BoxGeometry(0.03, 0.005, 0.009), 'lace', shoe, V(0, 0.072 - k * 0.012, 0.03 + k * 0.016));
    lace.rotation.x = -0.6;
  }
}

// ---------- 씬 ----------

// 원래 파츠를 공통 좌표의 스킨 메시로 옮겨 골격과 가중치를 부여한다.
const bones=[];const joints={};
function bone(name,parent,world){const b=new THREE.Bone();b.name=name;const at=new THREE.Vector3(...world);b.position.copy(parent?at.clone().sub(parent.userData.rest):at);b.userData.rest=at;bones.push(b);joints[name]=b;if(parent)parent.add(b);return b;}
const hips=bone('Hips',null,[0,.33,0]);const spine=bone('Spine',hips,[0,.51,0]);const skull=bone('HeadBone',spine,[0,.765,0]);
for(const [side,s] of [['Right',1],['Left',-1]]){
 const arm=bone(side+'UpperArm',spine,[s*.095,.515,0]);const fore=bone(side+'ForeArm',arm,[s*.205,.435,.007]);bone(side+'Hand',fore,[s*.285,.375,.012]);
 const leg=bone(side+'UpperLeg',hips,[s*.058,.32,0]);const calf=bone(side+'LowerLeg',leg,[s*.062,.2,0]);bone(side+'Foot',calf,[s*.068,.055,.015]);
 const tail=bone(side+'Tail',skull,[s*.16,.865,-.085]);bone(side+'TailTip',tail,[s*.28,.645,-.08]);
}
character.updateMatrixWorld(true);
const meshes=[];character.traverse(m=>{if(m.isMesh)meshes.push(m);});
const skeleton=new THREE.Skeleton(bones);character.add(hips);character.updateMatrixWorld(true);skeleton.calculateInverses();
for(const mesh of meshes){
 const geo=mesh.geometry.clone();geo.applyMatrix4(mesh.matrixWorld);const p=geo.attributes.position;const indices=[],weights=[];
 for(let i=0;i<p.count;i++){
  const x=p.getX(i),y=p.getY(i),side=x>=0?'Right':'Left';let a='Spine',b='Hips',w=THREE.MathUtils.clamp((y-.33)/.17,0,1);
  if(mesh.name.startsWith('Hair_Tail')){a=side+'Tail';b=side+'TailTip';w=THREE.MathUtils.clamp((y-.60)/.22,0,1);}
  else if(mesh.parent===head){a=b='HeadBone';w=1;}
  else if(/Shoulder|Sleeve|Cuff_|Hand_|Thumb_/.test(mesh.name)&&!mesh.name.startsWith('Jeans')){
   if(y>.44){a=side+'UpperArm';b=side+'ForeArm';w=THREE.MathUtils.clamp((y-.435)/.08,0,1);}
   else{a=side+'ForeArm';b=side+'Hand';w=THREE.MathUtils.clamp((y-.38)/.055,0,1);}
  }else if(/Leg_|Jeans_Cuff|Shoe_/.test(mesh.name)){
   if(y>.2){a=side+'UpperLeg';b=side+'LowerLeg';w=THREE.MathUtils.clamp((y-.2)/.12,0,1);}
   else{a=side+'LowerLeg';b=side+'Foot';w=THREE.MathUtils.clamp((y-.07)/.13,0,1);}
  }
  indices.push(bones.indexOf(joints[a]),bones.indexOf(joints[b]),0,0);weights.push(w,1-w,0,0);
 }
 geo.setAttribute('skinIndex',new THREE.Uint16BufferAttribute(indices,4));geo.setAttribute('skinWeight',new THREE.Float32BufferAttribute(weights,4));
 const skin=new THREE.SkinnedMesh(geo,mesh.material);skin.name=mesh.name;skin.castShadow=true;skin.frustumCulled=false;
 character.add(skin);skin.bind(skeleton,new THREE.Matrix4());mesh.removeFromParent();
}
head.removeFromParent();
// 머리카락 결은 원본 덩어리와 별도로 가는 선형 입체 파츠를 더한다.
for(const s of [-1,1])for(let i=0;i<3;i++){
 const g=new THREE.CylinderGeometry(.003,.001,.17,4).rotateZ(s*.18).translate(s*(.235+i*.012),.71,-.112);
 const ix=bones.indexOf(joints[(s>0?'Right':'Left')+'Tail']);const n=g.attributes.position.count;
 g.setAttribute('skinIndex',new THREE.Uint16BufferAttribute(Array.from({length:n},()=>[ix,0,0,0]).flat(),4));g.setAttribute('skinWeight',new THREE.Float32BufferAttribute(Array.from({length:n},()=>[1,0,0,0]).flat(),4));
 const m=new THREE.SkinnedMesh(g,new THREE.MeshStandardMaterial({color:0xd3ac43,flatShading:true,roughness:.9}));m.name='HairStrand_'+s+'_'+i;character.add(m);m.bind(skeleton,new THREE.Matrix4());
}
const clips=[];
function motion(name,duration,pose){const tracks=[];const count=25;const times=Array.from({length:count},(_,i)=>duration*i/(count-1));
 for(const b of bones){const values=[];for(let i=0;i<count;i++){const q=new THREE.Quaternion().setFromEuler(new THREE.Euler(...pose(b.name,i/(count-1))));values.push(q.x,q.y,q.z,q.w);}tracks.push(new THREE.QuaternionKeyframeTrack(b.name+'.quaternion',times,values));}
 clips.push(new THREE.AnimationClip(name,duration,tracks));}
const wave=u=>Math.sin(u*Math.PI*2),pulse=u=>Math.sin(u*Math.PI);
motion('Idle',2,(n,u)=>n==='Spine'?[.025*wave(u),0,0]:n.includes('Tail')?[.04*wave(u),0,.025*wave(u)]:[0,0,0]);
motion('Run',.65,(n,u)=>n.includes('UpperLeg')?[.65*wave(u)*(n.startsWith('Right')?1:-1),0,0]:n.includes('LowerLeg')?[Math.max(0,wave(u+(n.startsWith('Right')?0:.5)))*.8,0,0]:n.includes('UpperArm')?[-.6*wave(u)*(n.startsWith('Right')?1:-1),0,0]:n==='Spine'?[.12,0,0]:[0,0,0]);
motion('Dodge',.22,(n,u)=>n==='Spine'?[.5*pulse(u),0,0]:n.includes('UpperLeg')?[-.45*pulse(u),0,0]:n.includes('LowerLeg')?[.9*pulse(u),0,0]:n.includes('UpperArm')?[-.6*pulse(u),0,0]:[0,0,0]);
motion('Jump',.65,(n,u)=>n.includes('UpperLeg')?[-.5*pulse(u),0,0]:n.includes('LowerLeg')?[.8*pulse(u),0,0]:n.includes('UpperArm')?[-.7*pulse(u),0,0]:[0,0,0]);
motion('Land',.16,(n,u)=>n==='Spine'?[.25*pulse(u),0,0]:n.includes('LowerLeg')?[.45*pulse(u),0,0]:[0,0,0]);
for(const side of [1,-1])motion(side>0?'ParryLeft':'ParryRight',.4,(n,u)=>n==='RightUpperArm'?[-1.35*pulse(u),side*(u-.5)*2.3*pulse(u),-.4*pulse(u)]:n==='RightForeArm'?[-.75*pulse(u),side*.6*pulse(u),0]:n==='Spine'?[0,side*.22*wave(u),0]:[0,0,0]);
motion('Brace',1,(n,u)=>n==='Spine'?[.3,0,0]:n.includes('UpperArm')?[-.95,0,n.startsWith('Right')?.4:-.4]:n.includes('ForeArm')?[-1,0,0]:n.includes('LowerLeg')?[.35,0,0]:n.includes('UpperLeg')?[-.2,0,0]:[0,0,0]);
motion('Hit',.28,(n,u)=>n==='Spine'?[-.35*pulse(u),0,.12*pulse(u)]:n==='HeadBone'?[-.15*pulse(u),0,0]:[0,0,0]);
motion('Fall',.65,(n,u)=>n==='Hips'?[-Math.PI*.48*u,0,0]:n.includes('UpperArm')?[-.5*u,0,0]:[0,0,0]);

return {character,clips,bones,triangleCount};
}
