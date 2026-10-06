import fs from 'node:fs';
const input=fs.readFileSync('Preview/heroine_preview.html','utf8');
let model=input.slice(input.indexOf('const COLORS ='),input.indexOf("const view = document.getElementById"));
// v1은 유지하고 같은 파츠 제작 코드를 v2의 공통 모듈로 추출한다.
model=model.replace('0.0035 + layer * 0.0012','0.0005 + layer * 0.00025');
model=model.replace(/function faceZ\(x, y\) \{[\s\S]*?\n\}/,`function faceZ(x,y) {
  const face=head.children.find(p=>p.name==='Face');
  const surface=new THREE.Mesh(face.geometry,face.material);
  const ray=new THREE.Raycaster(new THREE.Vector3(x,y,1),new THREE.Vector3(0,0,-1));
  const hit=ray.intersectObject(surface,false)[0];return hit?hit.point.z:0;
}`);
model=model.replace('scale(0.034, 0.016, 0.026)','scale(0.038, 0.024, 0.032)');
const rig=fs.readFileSync('Preview/heroine_rig_v2.js','utf8');
fs.writeFileSync('Preview/heroine_model_v2.js',`import * as THREE from './vendor/three/build/three.module.js';\nexport function createHeroine(){\n${model}\n${rig}\nreturn {character,clips,bones,triangleCount};\n}\n`);
let html=input.replaceAll('로우폴리 모델 v1','로우폴리 모델 v2').replace('선 자세 (리깅 전)','리깅 · 오른손 쳐내기 · 동작 10종');
html=html.replace('https://cdn.jsdelivr.net/npm/three@0.170.0/build/three.module.js','./vendor/three/build/three.module.js').replace('https://cdn.jsdelivr.net/npm/three@0.170.0/examples/jsm/','./vendor/three/examples/jsm/');
const start=html.indexOf('// ---------- 팔레트');const end=html.indexOf("const view = document.getElementById");
html=html.slice(0,start)+`import { createHeroine } from './heroine_model_v2.js';\nconst {character,clips,bones,triangleCount}=createHeroine();\nconst materials={};character.traverse(m=>{if(m.isMesh)materials[m.material.name]=m.material;});\n`+html.slice(end);
html=html.replace("const GLB_NAME = 'heroine_v1.glb'","const GLB_NAME = 'heroine_v2.glb'").replace('{ binary: true }','{ binary: true, animations:clips }');
html=html.replace('<button id="export">','<select id="motion" aria-label="동작 선택"></select><button id="export">');
html=html.replace('renderer.setAnimationLoop(() => {',`const mixer=new THREE.AnimationMixer(character);const clock=new THREE.Clock();\nconst motion=document.getElementById('motion');const names=['대기','달리기','회피','점프','착지','쳐내기 왼쪽에서','쳐내기 오른쪽에서','정지 자세','피격','쓰러짐'];\nclips.forEach((c,i)=>motion.add(new Option(names[i],c.name)));\nfunction play(){mixer.stopAllAction();mixer.clipAction(clips.find(c=>c.name===motion.value)).play();}\nmotion.addEventListener('change',play);play();\nrenderer.setAnimationLoop(() => {mixer.update(Math.min(.05,clock.getDelta()));`);
html=html.replace('camera, controls, exportGlb','camera, controls, exportGlb, mixer, clips');
fs.writeFileSync('Preview/heroine_preview_v2.html',html);
console.log('v2 공통 모델과 미리보기 생성 완료');
