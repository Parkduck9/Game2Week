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
