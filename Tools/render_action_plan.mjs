// MD가 기준이다. 수정 후 node Tools/render_action_plan.mjs 로 HTML을 갱신한다.
// 외부 패키지/서버 없이 읽을 수 있는 문서와 단계별 비교 보기를 생성한다.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const sourcePath = path.join(root, 'Plans/Action_Balance_Plan.md');
const targetPath = path.join(root, 'Plans/Action_Balance_Plan.html');
const md = fs.readFileSync(sourcePath, 'utf8');
const statusSummary = md.match(/상태: (.+?) · 수치:/)?.[1] ?? '설계 초안';
const escape = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
function inline(s) {
  const codes = [];
  s = s.replace(/`([^`]+)`/g, (_, v) => `\u0000${codes.push(`<code>${escape(v)}</code>`) - 1}\u0000`);
  s = escape(s).replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2">$1</a>').replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
  return s.replace(/\u0000(\d+)\u0000/g, (_, n) => codes[n]);
}
const lines = md.split(/\r?\n/);
const toc = [];
let rendered = '';
for (let i = 0; i < lines.length;) {
  const line = lines[i];
  if (!line.trim()) { i++; continue; }
  if (line.startsWith('```')) {
    const block = []; i++;
    while (i < lines.length && !lines[i].startsWith('```')) block.push(lines[i++]);
    i++; rendered += `<pre><code>${escape(block.join('\n'))}</code></pre>`; continue;
  }
  const heading = line.match(/^(#{1,3}) (.+)$/);
  if (heading) {
    const level = heading[1].length;
    if (level === 1) { i++; continue; }
    const id = `section-${i}`;
    if (level === 2) toc.push({id, text: heading[2]});
    rendered += `<h${level} id="${id}">${inline(heading[2])}</h${level}>`; i++; continue;
  }
  if (line.startsWith('|')) {
    const rows = [];
    while (i < lines.length && lines[i].startsWith('|')) {
      const cells = lines[i++].split('|').slice(1, -1).map(s => s.trim());
      if (cells.every(s => /^:?-+:?$/.test(s))) continue;
      rows.push(cells);
    }
    rendered += '<div class="table-scroll" tabindex="0" role="region" aria-label="설계 표"><table><thead><tr>' + rows[0].map(s => `<th scope="col">${inline(s)}</th>`).join('') + '</tr></thead><tbody>' + rows.slice(1).map(row => '<tr>' + row.map(s => `<td>${inline(s)}</td>`).join('') + '</tr>').join('') + '</tbody></table></div>';
    continue;
  }
  const list = line.match(/^(- |\d+\. )/);
  if (list) {
    const ordered = /^\d/.test(line); const tag = ordered ? 'ol' : 'ul';
    rendered += `<${tag}>`;
    while (i < lines.length && (ordered ? /^\d+\. / : /^- /).test(lines[i])) rendered += `<li>${inline(lines[i++].replace(/^(- |\d+\. )/, ''))}</li>`;
    rendered += `</${tag}>`; continue;
  }
  const paragraph = [];
  while (i < lines.length && lines[i].trim() && !/^(#|\||```|- |\d+\. )/.test(lines[i])) paragraph.push(lines[i++]);
  rendered += `<p>${inline(paragraph.join(' '))}</p>`;
}
const tiers = lines.filter(s => /^\| [1-8] \/ 1-/.test(s)).map(line => {
  const cells = line.split('|').slice(1, -1).map(s => s.trim());
  return {stage: cells[0], pattern: cells[1], count: Number(cells[2]), interval: Number.parseFloat(cells[3]), speed: Number.parseFloat(cells[4]), overlap: Number(cells[5])};
});
if (tiers.length !== 8 || tiers.some((t, i) => t.count !== i + 1 || (i > 0 && t.interval >= tiers[i - 1].interval))) throw new Error('8단계 누적/간격 표를 확인하세요.');
const nav = toc.map(h => `<a href="#${h.id}">${escape(h.text)}</a>`).join('');
const html = `<!doctype html>
<html lang="ko"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>패턴 누적 · 색 대응 · 3D 액션 회피 설계</title>
<style>
:root{color-scheme:dark;--bg:#10151e;--panel:#192231;--line:#334154;--text:#ecf2fa;--muted:#acbdd0;--blue:#83bcff;--mint:#83e4c4;--yellow:#f7d788}
*{box-sizing:border-box}html{scroll-behavior:smooth;scroll-padding-top:20px}body{margin:0;background:var(--bg);color:var(--text);font:15px/1.8 'Malgun Gothic',system-ui,sans-serif}a{color:var(--blue)}a:hover{color:var(--mint)}a:focus-visible,button:focus-visible,input:focus-visible,summary:focus-visible{outline:3px solid var(--mint);outline-offset:4px}
.shell{max-width:1320px;margin:auto;padding:45px 28px 70px}.eyebrow{letter-spacing:.12em;color:var(--mint);font-size:12px;font-weight:700}h1{font-size:clamp(27px,4vw,44px);line-height:1.35;margin:14px 0}h2{font-size:23px;margin:50px 0 15px;border-bottom:1px solid var(--line);padding-bottom:10px}h3{font-size:18px;margin:30px 0 10px}p{margin:12px 0}.lead{max-width:850px;color:var(--muted);font-size:17px}.badge{display:inline-block;background:#343021;color:var(--yellow);border:1px solid #645737;border-radius:5px;padding:3px 9px;font-size:12px}.actions{display:flex;flex-wrap:wrap;gap:10px;margin:20px 0 30px}.actions a{border:1px solid var(--line);padding:6px 12px;border-radius:6px;text-decoration:none}
.explorer{background:var(--panel);border:1px solid var(--line);border-radius:16px;padding:24px;margin:24px 0 32px}.explorer h2{border:0;margin:0;padding:0;font-size:20px}.explorer .note{color:var(--muted);font-size:13px}.stage-buttons{display:flex;gap:8px;flex-wrap:wrap;margin:20px 0}button{cursor:pointer;background:var(--bg);color:var(--text);border:1px solid var(--line);padding:9px 15px;border-radius:7px;font:inherit}button[aria-pressed=true]{background:var(--blue);color:#102031;border-color:var(--blue);font-weight:bold}.preview-grid{display:grid;grid-template-columns:1fr 1fr;gap:24px}.metrics{display:grid;grid-template-columns:1fr 1fr;gap:12px}.metric{background:#111b28;border:1px solid var(--line);border-radius:10px;padding:14px}.metric small{color:var(--muted);display:block}.metric strong{font-size:25px;font-variant-numeric:tabular-nums}.new-pattern{margin:12px 0;padding:10px 14px;border-left:3px solid var(--mint);background:#142c2c}.diagram{background:#101b28;border:1px solid var(--line);border-radius:10px;padding:14px}.diagram svg{width:100%;height:auto;display:block}.diagram p{margin:4px 0;color:var(--muted);font-size:13px}.pulse-track{height:28px;position:relative;border-radius:5px;background:#101b28;overflow:hidden;margin:14px 0 2px}.pulse{position:absolute;top:5px;bottom:5px;width:3px;background:var(--mint)}.pulse-label{display:flex;justify-content:space-between;color:var(--muted);font-size:12px}.pool{display:flex;flex-wrap:wrap;gap:6px;margin-top:12px}.pool span{padding:3px 8px;background:#25364b;border-radius:5px;font-size:12px}.pool span.new{border:1px solid var(--mint);color:var(--mint)}.layout{display:grid;grid-template-columns:215px minmax(0,1fr);gap:35px}.toc{position:sticky;top:25px;align-self:start;max-height:90vh;overflow:auto}.toc strong{display:block;margin-bottom:10px;color:var(--muted);font-size:13px}.toc a{display:block;padding:5px 0;font-size:13px;text-decoration:none}article{min-width:0}article>h2:first-of-type{margin-top:22px}.table-scroll{overflow-x:auto;border:1px solid var(--line);border-radius:8px;margin:16px 0}table{border-collapse:collapse;width:100%;min-width:640px;font-size:13px}th,td{padding:12px 13px;text-align:left;border-bottom:1px solid var(--line);vertical-align:top}th{background:#223044;color:var(--blue)}tr:last-child td{border-bottom:0}tbody tr:nth-child(even){background:#151e2b}ul,ol{padding-left:23px}li{margin:9px 0}code{background:#223044;padding:2px 5px;border-radius:4px;font:13px/1.6 Consolas,monospace}pre{overflow:auto;background:#192231;border:1px solid var(--line);padding:20px;border-radius:8px}pre code{background:none;padding:0;white-space:pre}.footer{border-top:1px solid var(--line);padding-top:20px;margin-top:45px;color:var(--muted);font-size:13px}
@media(max-width:850px){.layout{grid-template-columns:1fr}.toc{position:static;max-height:none;display:flex;flex-wrap:wrap;gap:4px 16px}.toc strong{width:100%}.preview-grid{grid-template-columns:1fr}.shell{padding:25px 18px 50px}h2{font-size:20px}.explorer{padding:18px}}
@media print{ :root{color-scheme:light;--bg:white;--panel:white;--text:#111;--muted:#444;--line:#bbb;--blue:#174772;--mint:#14563f}body{font-size:10pt}.shell{padding:0}.toc,.actions,.stage-buttons,.pulse-track,.pulse-label{display:none}.layout,.preview-grid{display:block}.explorer{break-inside:avoid}.metric,.diagram,.new-pattern,th,tbody tr:nth-child(even){background:white;color:#111}h2{break-after:avoid}table{min-width:0}pre{white-space:pre-wrap;background:white}a{color:#111}.badge{background:white;color:#111}}
</style></head><body><div class="shell">
<header><div class="eyebrow">GAME2WEEK / COMBAT DESIGN / 2026.10.06</div><h1>한 단계씩 늘어나는 패턴.<br>몸으로 피하는 3D 전투.</h1><span class="badge">${escape(statusSummary)} · 수치는 검증용 초안</span><p class="lead">최신 방향: 캐릭터 뒤 허리 카메라·마우스 회전/록온, 노랑 우클릭 쳐내기·Shift 회피·점프, 후속 Ctrl 빨강 정지·파랑 이동. 아래 8단계 간격은 비교용 초안입니다.</p><div class="actions"><a href="Action_Balance_Plan.md">기준 MD 읽기</a><a href="../TodoList.html">작업 대시보드</a><a href="#${toc.find(h => h.text.startsWith('9.')).id}">제작 순서 보기</a></div></header>
<section class="explorer" aria-labelledby="explorer-title"><h2 id="explorer-title">단계별 밸런스 비교</h2><p class="note">단계를 선택하면 누적 패턴과 기준 발사 간격을 비교할 수 있습니다. 게임 시뮬레이션이 아닌 설계 시각화입니다.</p><div class="stage-buttons" role="group" aria-label="비교할 단계">${tiers.map((_,i)=>`<button type="button" data-tier="${i}" aria-pressed="${i===0}">1-${i+1}</button>`).join('')}</div>
<div class="preview-grid"><div><div id="stage-summary" aria-live="polite"><div class="metrics"><div class="metric"><small>사용 가능한 패턴</small><strong id="count">1종</strong></div><div class="metric"><small>기준 발사 간격</small><strong id="interval">1.40초</strong></div><div class="metric"><small>동시 공격 상한</small><strong id="overlap">1종</strong></div><div class="metric"><small>직선 기준 탄속</small><strong id="speed">2.4m/s</strong></div></div><p class="new-pattern" id="new-pattern">새 패턴: P1 원형 탄막</p></div><div class="pulse-track" id="pulse-track" aria-hidden="true"></div><div class="pulse-label"><span>기준 공격 발사 시점 / 시작 0초</span><span>6초</span></div><div class="pool" id="pool"></div><p class="note">누적 종류 ≠ 동시 실행 수. 위험 구역은 자체 최소 반복 간격을 유지하고 새 패턴은 먼저 단독으로 소개합니다.</p></div>
<div class="diagram"><svg viewBox="0 0 520 300" role="img" aria-labelledby="camera-title camera-desc"><title id="camera-title">허리 카메라와 노랑 대응 개념도</title><desc id="camera-desc">입체 경기장에서 플레이어가 노란 공격을 쳐내거나 옆으로 회피하는 개념을 나타냅니다.</desc><defs><linearGradient id="floor" x2="0" y2="1"><stop stop-color="#2b4a63"/><stop offset="1" stop-color="#152636"/></linearGradient><marker id="arrow" markerWidth="8" markerHeight="8" refX="5" refY="3" orient="auto"><path d="M0 0L6 3L0 6" fill="none" stroke="#83e4c4"/></marker></defs><path d="M100 110L400 110L490 265L20 265Z" fill="url(#floor)" stroke="#648299"/><g stroke="#3b566b" stroke-width="1"><path d="M85 136H415M65 172H440M43 216H465M175 110L142 265M250 110V265M325 110L368 265"/></g><ellipse cx="262" cy="153" rx="99" ry="25" fill="none" stroke="#f7d788" stroke-width="4"/><path d="M244 97L262 70L281 97L262 121Z" fill="#b69cf1"/><text x="296" y="93" fill="#ecf2fa" font-size="14">적</text><ellipse cx="218" cy="231" rx="21" ry="8" fill="#83bcff" opacity=".3"/><path d="M201 193L209 218H227L235 193Z" fill="#83bcff"/><circle cx="218" cy="182" r="12" fill="#f7d788"/><path d="M223 207Q266 122 302 205" fill="none" stroke="#f7d788" stroke-dasharray="5 5" stroke-width="2"/><text x="265" y="132" fill="#f7d788" font-size="13">노랑: 쳐내기 또는 회피</text><path d="M246 232H359" fill="none" stroke="#83e4c4" stroke-width="3" marker-end="url(#arrow)"/><text x="306" y="222" fill="#83e4c4" font-size="13">대시</text><path d="M168 294L177 273L196 294Z" fill="#acbdd0"/><text x="206" y="291" fill="#acbdd0" font-size="13">최신 목표: 뒤 허리 카메라</text></svg><p>그림은 회피 개념도입니다. 최신 목표는 뒤 허리 카메라이며, 회전/록온과 오른손 쳐내기 방향은 본문 0절에 정리했습니다.</p></div></div></section>
<div class="layout"><nav class="toc" aria-label="문서 목차"><strong>설계 목차</strong>${nav}</nav><article>${rendered}</article></div><footer class="footer">기준: Plans/Action_Balance_Plan.md · HTML 갱신: node Tools/render_action_plan.mjs · 코드/씬 적용 여부는 TODO와 WORKLOG에서 확인합니다.</footer></div>
<script>
const tiers=${JSON.stringify(tiers)};
function showTier(index){const tier=tiers[index];document.querySelectorAll('[data-tier]').forEach(b=>b.setAttribute('aria-pressed',String(Number(b.dataset.tier)===index)));document.getElementById('count').textContent=tier.count+'종';document.getElementById('interval').textContent=tier.interval.toFixed(2)+'초';document.getElementById('overlap').textContent=tier.overlap+'종';document.getElementById('speed').textContent=tier.speed.toFixed(1)+'m/s';document.getElementById('new-pattern').textContent=tier.stage+' · 새 패턴: '+tier.pattern;const track=document.getElementById('pulse-track');track.replaceChildren();for(let t=0;t<6;t+=tier.interval){const mark=document.createElement('span');mark.className='pulse';mark.style.left=(t/6*100)+'%';track.append(mark);}const pool=document.getElementById('pool');pool.replaceChildren();tiers.slice(0,index+1).forEach((t,i)=>{const chip=document.createElement('span');chip.className=i===index?'new':'';chip.textContent=t.pattern;pool.append(chip);});}
document.querySelectorAll('[data-tier]').forEach(b=>b.addEventListener('click',()=>showTier(Number(b.dataset.tier))));showTier(0);
</script></body></html>`;
fs.writeFileSync(targetPath, html, 'utf8');
console.log(`Generated ${path.relative(root, targetPath)} (${tiers.length} tiers, ${toc.length} sections)`);
