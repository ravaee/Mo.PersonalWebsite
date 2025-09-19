// Lightweight particle system (no external lib)
(function(){
  const host = document.getElementById('particle-background');
  if(!host) return;
  const canvas = document.createElement('canvas');
  canvas.className = 'particle-canvas';
  host.appendChild(canvas);
  const ctx = canvas.getContext('2d');
  let w=canvas.width=host.clientWidth; let h=canvas.height=host.clientHeight;
  window.addEventListener('resize',()=>{w=canvas.width=host.clientWidth;h=canvas.height=host.clientHeight;init();});

  const PARTICLE_COUNT = Math.min(90, Math.floor(w*h/18000));
  const particles=[];
  function rand(a,b){return Math.random()*(b-a)+a;}
  function init(){
    particles.length=0;
    for(let i=0;i<PARTICLE_COUNT;i++){
      particles.push({
        x: rand(0,w),
        y: rand(0,h),
        r: rand(1,3.2),
        vx: rand(-0.25,0.25),
        vy: rand(-0.25,0.25),
        o: rand(0.15,0.55)
      });
    }
  }
  function step(){
    ctx.clearRect(0,0,w,h);
    // subtle gradient overlay
    const grd = ctx.createLinearGradient(0,0,w,h);
    grd.addColorStop(0,'rgba(102,126,234,0.05)');
    grd.addColorStop(1,'rgba(118,75,162,0.05)');
    ctx.fillStyle=grd;ctx.fillRect(0,0,w,h);
    // draw lines
    for(let i=0;i<particles.length;i++){
      const p=particles[i];
      for(let j=i+1;j<particles.length;j++){
        const q=particles[j];
        const dx=p.x-q.x, dy=p.y-q.y; const d=dx*dx+dy*dy;
        if(d<9000){
          const alpha = 1 - d/9000;
            ctx.strokeStyle = 'rgba(118,75,162,'+(alpha*0.25)+')';
            ctx.lineWidth=1;
            ctx.beginPath();
            ctx.moveTo(p.x,p.y);ctx.lineTo(q.x,q.y);ctx.stroke();
        }
      }
    }
    // draw particles
    for(const p of particles){
      p.x+=p.vx; p.y+=p.vy;
      if(p.x<0||p.x>w) p.vx*=-1;
      if(p.y<0||p.y>h) p.vy*=-1;
      ctx.beginPath();
      ctx.fillStyle='rgba(91,60,196,'+p.o+')';
      ctx.arc(p.x,p.y,p.r,0,Math.PI*2);
      ctx.fill();
    }
    requestAnimationFrame(step);
  }
  init();
  step();
})();
