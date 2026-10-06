import fs from 'node:fs';
import {createHeroine} from './heroine_model_v2.js';
import {GLTFExporter} from './vendor/three/examples/jsm/exporters/GLTFExporter.js';
// GLTFExporter가 브라우저 FileReader를 사용하므로 Node에서 같은 비동기 인터페이스를 제공한다.
globalThis.FileReader=class {readAsArrayBuffer(blob){blob.arrayBuffer().then(v=>{this.result=v;this.onloadend?.();});}readAsDataURL(blob){blob.arrayBuffer().then(v=>{this.result='data:application/octet-stream;base64,'+Buffer.from(v).toString('base64');this.onloadend?.();});}};
const {character,clips,bones}=createHeroine();
const glb=await new GLTFExporter().parseAsync(character,{binary:true,animations:clips});
fs.mkdirSync('Exports',{recursive:true});fs.writeFileSync('Exports/heroine_v2.glb',Buffer.from(glb));
fs.copyFileSync('Exports/heroine_v2.glb','Assets/_Project/Art/Characters/Heroine/heroine_v2.glb');
console.log(JSON.stringify({bones:bones.length,clips:clips.map(c=>c.name),bytes:glb.byteLength}));
