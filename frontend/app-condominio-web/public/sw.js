const CACHE_VERSION='conjunto-al-dia-v1';
const SHELL_CACHE=`${CACHE_VERSION}-shell`;
const RUNTIME_CACHE=`${CACHE_VERSION}-runtime`;
const SHELL=['/','/manifest.webmanifest','/icons/app-icon.svg'];

self.addEventListener('install',event=>{
  event.waitUntil(caches.open(SHELL_CACHE).then(cache=>cache.addAll(SHELL)).then(()=>self.skipWaiting()));
});

self.addEventListener('activate',event=>{
  event.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(k=>!k.startsWith(CACHE_VERSION)).map(k=>caches.delete(k)))).then(()=>self.clients.claim()));
});

self.addEventListener('fetch',event=>{
  const request=event.request;
  if(request.method!=='GET') return;
  const url=new URL(request.url);
  if(url.origin!==self.location.origin) return;
  if(url.pathname.startsWith('/api/')) return;
  if(request.headers.has('authorization')) return;

  if(request.mode==='navigate'){
    event.respondWith(fetch(request).then(response=>{
      if(response.ok){const copy=response.clone();caches.open(SHELL_CACHE).then(cache=>cache.put('/',copy));}
      return response;
    }).catch(()=>caches.match('/')));
    return;
  }

  if(['script','style','font','image'].includes(request.destination)||url.pathname==='/manifest.webmanifest'){
    event.respondWith(caches.match(request).then(cached=>cached||fetch(request).then(response=>{
      if(response.ok&&response.type==='basic'){const copy=response.clone();caches.open(RUNTIME_CACHE).then(cache=>cache.put(request,copy));}
      return response;
    })));
  }
});