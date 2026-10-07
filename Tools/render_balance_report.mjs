// 자동 플레이 측정 리포트 JSON → HTML (5단계, 9단계: 시작 거리·시간 초과·겹·체력 단계). 측정: BalanceMeasurementTests.MeasureAllStages
// 사용: node Tools/render_balance_report.mjs [Logs/balance_report.json] → Plans/Balance_Report.html
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const source = path.resolve(root, process.argv[2] ?? 'Logs/balance_report.json');
const target = path.join(root, 'Plans/Balance_Report.html');
const report = JSON.parse(fs.readFileSync(source, 'utf8').replace(/^﻿/, ''));
const esc = s => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
const stages = report.stages ?? [];
const max = key => Math.max(1, ...stages.map(s => s[key] ?? 0));
const bar = (value, key, cls) => `<div class="cell"><span class="bar ${cls}" style="width:${(value / max(key)) * 100}%"></span><b>${value}</b></div>`;
const rows = stages.map(s => {
  const turns = Math.max(1, s.enemyTurns);
  return `<tr>
    <td><b>${esc(s.stageName)}</b><br><small>${esc(s.stageId)}</small></td>
    <td>${bar(s.hits, 'hits', 'hit')}<small>턴당 ${(s.hits / turns).toFixed(1)}</small></td>
    <td>${s.contacts}/${s.enemyTurns}<br><small>평균 ${s.averageApproachSeconds.toFixed(1)}초 · 시작 거리 ${(s.startDistance ?? 0).toFixed(0)}m · 시간 초과 ${s.timeouts ?? 0}</small></td>
    <td>${s.maxLayers ?? 0}겹 · 단계 ${s.maxPhase ?? 0}</td>
    <td>${s.dodges} · ${s.parries} · ${s.jumps}</td>
    <td>${s.braces} · <span class="red">${s.redPasses}</span> / <span class="blue">${s.bluePasses}</span></td>
    <td>${s.hpLeft}/${s.hpMax}</td>
    <td>${esc(s.outcome)}<br><small>${s.battleSeconds.toFixed(0)}초</small></td></tr>`;
}).join('');

const html = `<!doctype html>
<html lang="ko"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>밸런스 측정 리포트</title>
<style>
:root{--bg:#f7f7f4;--panel:#fff;--text:#1f2328;--muted:#5d6670;--line:#dcdfe3;--hit:#d9534f;--red:#d33;--blue:#2f6fd6}
@media (prefers-color-scheme:dark){:root:not([data-theme="light"]){--bg:#16181c;--panel:#1e2127;--text:#e6e8eb;--muted:#9aa3ad;--line:#333842;--hit:#ef7b77;--red:#ff6b6b;--blue:#7fb0ff}}
:root[data-theme="dark"]{--bg:#16181c;--panel:#1e2127;--text:#e6e8eb;--muted:#9aa3ad;--line:#333842;--hit:#ef7b77;--red:#ff6b6b;--blue:#7fb0ff}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--text);font:15px/1.6 "Pretendard","Malgun Gothic",system-ui,sans-serif}
main{max-width:1100px;margin:0 auto;padding:28px 16px 60px}h1{font-size:24px;margin:0 0 4px}p{color:var(--muted);margin:4px 0 14px}
.table{overflow-x:auto}table{border-collapse:collapse;width:100%;background:var(--panel)}th,td{border:1px solid var(--line);padding:8px 10px;text-align:left;vertical-align:top}
th{font-weight:600;font-size:13px;color:var(--muted)}small{color:var(--muted)}.cell{position:relative;min-width:110px}
.bar{display:block;height:8px;border-radius:4px;margin-bottom:3px}.bar.hit{background:var(--hit)}.red{color:var(--red)}.blue{color:var(--blue)}
</style></head><body><main>
<h1>밸런스 측정 리포트</h1>
<p>측정 ${esc(report.createdAt ?? '')} · 맵 ${stages.length}개 · ${esc(report.note ?? '')}</p>
<div class="table"><table><thead><tr><th>맵</th><th>피격</th><th>적 접근 성공/턴</th><th>최대 동시 공격 · 체력 단계</th><th>회피 · 쳐내기 · 점프</th><th>정지 자세 · 통과 빨강/파랑</th><th>남은 HP</th><th>결과</th></tr></thead>
<tbody>${rows}</tbody></table></div>
<p>원본: ${esc(path.relative(root, source))} · node Tools/render_balance_report.mjs 로 생성</p>
</main></body></html>
`;
fs.writeFileSync(target, html, 'utf8');
console.log(`written ${path.relative(root, target)} (${stages.length} stages)`);
