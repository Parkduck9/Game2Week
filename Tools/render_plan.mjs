// 범용 계획 문서 MD → HTML. MD가 기준이며, 수정 후 다시 실행해 HTML을 갱신한다.
// 사용: node Tools/render_plan.mjs Plans/Parallel_Work_Plan.md
// (Action_Balance_Plan은 단계 비교 화면이 있어 Tools/render_action_plan.mjs를 쓴다)
import fs from 'node:fs';
import path from 'node:path';

const source = process.argv[2];
if (!source) { console.error('usage: node Tools/render_plan.mjs <file.md>'); process.exit(1); }
const target = source.replace(/\.md$/i, '.html');
const md = fs.readFileSync(source, 'utf8');
const escape = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
function inline(s) {
  const codes = [];
  s = s.replace(/`([^`]+)`/g, (_, v) => `\u0000${codes.push(`<code>${escape(v)}</code>`) - 1}\u0000`);
  s = escape(s)
    .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2">$1</a>')
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
  return s.replace(/\u0000(\d+)\u0000/g, (_, n) => codes[n]);
}
const cells = row => row.trim().replace(/^\||\|$/g, '').split('|').map(c => c.trim());

const lines = md.split(/\r?\n/);
let title = path.basename(source, '.md');
const toc = [];
let body = '';
for (let i = 0; i < lines.length;) {
  const line = lines[i];
  if (!line.trim()) { i++; continue; }
  if (line.startsWith('```')) {
    const block = []; i++;
    while (i < lines.length && !lines[i].startsWith('```')) block.push(lines[i++]);
    i++; body += `<pre><code>${escape(block.join('\n'))}</code></pre>`; continue;
  }
  const heading = line.match(/^(#{1,4}) (.+)$/);
  if (heading) {
    const level = heading[1].length; i++;
    if (level === 1) { title = heading[2]; continue; }
    const id = `s${i}`;
    if (level === 2) toc.push({ id, text: heading[2] });
    body += `<h${level} id="${id}">${inline(heading[2])}</h${level}>`; continue;
  }
  if (line.startsWith('|')) {
    const rows = [];
    while (i < lines.length && lines[i].startsWith('|')) rows.push(lines[i++]);
    const head = cells(rows[0]);
    const data = rows.slice(2).map(cells);
    body += `<div class="table"><table><thead><tr>${head.map(c => `<th>${inline(c)}</th>`).join('')}</tr></thead><tbody>` +
      data.map(r => `<tr>${r.map(c => `<td>${inline(c)}</td>`).join('')}</tr>`).join('') + '</tbody></table></div>';
    continue;
  }
  const listItem = line.match(/^(\s*)(-|\d+\.) (.+)$/);
  if (listItem) {
    const ordered = /\d/.test(listItem[2]);
    const items = [];
    while (i < lines.length && /^(\s*)(-|\d+\.) /.test(lines[i])) items.push(lines[i++].replace(/^(\s*)(-|\d+\.) /, ''));
    const tag = ordered ? 'ol' : 'ul';
    body += `<${tag}>${items.map(t => `<li>${inline(t)}</li>`).join('')}</${tag}>`; continue;
  }
  const para = [];
  while (i < lines.length && lines[i].trim() && !/^(#|\||```|\s*(-|\d+\.) )/.test(lines[i])) para.push(lines[i++]);
  body += `<p>${inline(para.join(' '))}</p>`;
}

const html = `<!doctype html>
<html lang="ko"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>${escape(title)}</title>
<style>
:root{--bg:#f7f7f4;--panel:#fff;--text:#1f2328;--muted:#5d6670;--line:#dcdfe3;--accent:#c7901a;--code:#f0efe9}
@media (prefers-color-scheme:dark){:root:not([data-theme="light"]){--bg:#16181c;--panel:#1e2127;--text:#e6e8eb;--muted:#9aa3ad;--line:#333842;--accent:#f0c040;--code:#272b33}}
:root[data-theme="dark"]{--bg:#16181c;--panel:#1e2127;--text:#e6e8eb;--muted:#9aa3ad;--line:#333842;--accent:#f0c040;--code:#272b33}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--text);font:15px/1.65 "Pretendard","Malgun Gothic",system-ui,sans-serif}
main{max-width:1040px;margin:0 auto;padding:28px 16px 64px}h1{font-size:26px;margin:0 0 6px}
h2{font-size:20px;margin:36px 0 10px;padding-bottom:6px;border-bottom:2px solid var(--accent)}h3{font-size:16px;margin:22px 0 8px}
nav{display:flex;flex-wrap:wrap;gap:6px;margin:14px 0 8px}nav a{font-size:13px;padding:3px 10px;border:1px solid var(--line);border-radius:999px;color:var(--muted);text-decoration:none}
a{color:var(--accent)}code{background:var(--code);padding:1px 5px;border-radius:4px;font-size:.9em;word-break:break-all}
pre{background:var(--code);padding:12px 14px;border-radius:8px;overflow-x:auto}pre code{padding:0;word-break:normal;white-space:pre}
.table{overflow-x:auto;margin:10px 0}table{border-collapse:collapse;width:100%;background:var(--panel)}
th,td{border:1px solid var(--line);padding:7px 10px;text-align:left;vertical-align:top}th{background:var(--code);font-weight:600}
ul,ol{padding-left:22px}.src{color:var(--muted);font-size:13px}
</style></head><body><main>
<h1>${escape(title)}</h1><div class="src">원본: ${escape(path.basename(source))} · node Tools/render_plan.mjs 로 생성</div>
<nav>${toc.map(t => `<a href="#${t.id}">${escape(t.text)}</a>`).join('')}</nav>
${body}
</main></body></html>
`;
fs.writeFileSync(target, html, 'utf8');
console.log(`written ${target} (${toc.length} sections)`);
