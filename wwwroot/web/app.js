// Shared helpers for the local community site.
async function api(path) {
  const r = await fetch(path);
  if (!r.ok) throw new Error('HTTP ' + r.status);
  return r.json();
}

function esc(s) {
  return String(s ?? '').replace(/[&<>"']/g, c =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

function fmtNum(n) {
  n = Number(n) || 0;
  if (n >= 1e9) return (n / 1e9).toFixed(1) + 'b';
  if (n >= 1e6) return (n / 1e6).toFixed(1) + 'm';
  if (n >= 1e3) return (n / 1e3).toFixed(1) + 'k';
  return String(n);
}

function qs(name) {
  return new URLSearchParams(location.search).get(name);
}

// Deterministic gradient per name so rooms have stable "cover art".
const GRADS = [
  'linear-gradient(135deg,#e8632c,#8f2f0e)',
  'linear-gradient(135deg,#2c7de8,#0e3a8f)',
  'linear-gradient(135deg,#37b24d,#0e6b2f)',
  'linear-gradient(135deg,#9c36e8,#4a0e8f)',
  'linear-gradient(135deg,#e8a62c,#8f5a0e)',
  'linear-gradient(135deg,#e82c7d,#8f0e4a)',
  'linear-gradient(135deg,#2cc8e8,#0e5a8f)',
  'linear-gradient(135deg,#7a5af8,#2f1e8f)'
];
function gradFor(name) {
  let h = 0;
  for (const c of String(name)) h = (h * 31 + c.charCodeAt(0)) >>> 0;
  return GRADS[h % GRADS.length];
}
function initialOf(name) {
  return esc((String(name || '?').replace(/^\^/, '')[0] || '?').toUpperCase());
}

// Rooms-page card: header (avatar + ^name + @creator), cover, likes/visits footer.
function roomCard(room, creator) {
  const stats = room.Stats || {};
  return `
    <a class="room-card" href="room.html?name=${encodeURIComponent(room.Name)}">
      <div class="room-card-head">
        <div class="avatar">${initialOf(room.Name)}</div>
        <div><b>^${esc(room.Name)}</b><span>@${esc(creator || 'unknown')}</span></div>
      </div>
      <div class="room-cover" style="background:${gradFor(room.Name)}">^${esc(room.Name)}</div>
      <div class="room-card-foot">
        <span>👍 ${fmtNum(stats.CheerCount)}</span>
        <span>🚶 ${fmtNum(stats.VisitCount)} visits</span>
      </div>
    </a>`;
}

// Home feed post built from a room.
function feedPost(room, username) {
  const stats = room.Stats || {};
  return `
    <div class="post">
      <div class="post-head">
        <div class="avatar">${initialOf(username || room.Name)}</div>
        <div>
          <b>${esc(username || 'Someone')}</b>
          <span class="handle">@${esc((username || 'player').toLowerCase().replace(/\s+/g, ''))}</span>
          <div class="room-tag">^${esc(room.Name)}</div>
        </div>
        <span class="post-share">↗</span>
      </div>
      <a href="room.html?name=${encodeURIComponent(room.Name)}">
        <div class="post-img" style="background:${gradFor(room.Name)}">${esc(room.Description || ('^' + room.Name))}</div>
      </a>
      <div class="post-foot">
        <span>👍 ${fmtNum(stats.CheerCount)}</span>
        <span>🚶 ${fmtNum(stats.VisitCount)}</span>
        <span>💬 ${fmtNum(stats.FavoriteCount)}</span>
      </div>
    </div>`;
}

function playerRow(p) {
  return `
    <a class="post" style="display:block" href="player.html?id=${p.accountId}">
      <div class="post-head">
        <div class="avatar">${initialOf(p.displayName || p.username)}</div>
        <div>
          <b>${esc(p.displayName)}</b>
          <span class="handle">@${esc(p.username)}</span>
        </div>
        <span class="post-share chev">›</span>
      </div>
    </a>`;
}

function dismissBanner() {
  const b = document.getElementById('announce');
  if (b) b.style.display = 'none';
}
