"""Original deterministic quarry underscore and mechanical foley; no sampled music."""
from pathlib import Path
import wave
import numpy as np

ROOT=Path('Assets/0_ProjectData/Audio')
ROOT.mkdir(parents=True,exist_ok=True)
SR=24000
rng=np.random.default_rng(4127)
def save(name,x):
    x=np.asarray(x); x=np.clip(x,-.95,.95)
    with wave.open(str(ROOT/(name+'.wav')),'wb') as f:
        f.setnchannels(1 if x.ndim==1 else x.shape[1]); f.setsampwidth(2); f.setframerate(SR)
        f.writeframes((x*32767).astype('<i2').tobytes())
def hz(n): return 440*2**((n-69)/12)
N=48*SR
music=np.zeros((N,2),np.float64)
def add(note,at,duration,amp,pan=0,pad=False):
    t=np.arange(int(duration*SR))/SR; f=hz(note)
    if pad:
        envelope=np.sin(np.pi*t/duration)**2
        tone=(np.sin(2*np.pi*f*t)+.22*np.sin(2*np.pi*f*2*t)+.1*np.sin(2*np.pi*f*3*t))
    else:
        envelope=(1-np.exp(-t*60))*np.exp(-t*1.7)*(1-t/duration)**2
        tone=np.sin(2*np.pi*f*t+.55*np.sin(2*np.pi*f*2*t)*np.exp(-t*3))+.12*np.sin(2*np.pi*f*3*t)*np.exp(-t*4)
    a=tone*envelope*amp
    idx=(int(at*SR)+np.arange(len(t)))%N
    music[idx,0]+=a*np.sqrt((1-pan)/2); music[idx,1]+=a*np.sqrt((1+pan)/2)
# D minor - B flat - F - C: spacious, muted worksite underscore, 80 BPM.
chords=[[38,45,53,57],[34,41,50,53],[29,41,48,57],[36,43,52,55]]
for bar in range(16):
    chord=chords[(bar//2)%4]
    for j,note in enumerate(chord): add(note,bar*3-1.5,6,.028,(-.5+j/3),True)
    for beat,j in [(0,2),(1.5,3)]:
        add(chord[j]+12,bar*3+beat,3,.055,(-.28 if j==2 else .28))
        add(chord[j]+12,bar*3+beat+.375,2,.012,(-.35 if j==3 else .35))
music*=.48/max(np.max(np.abs(music)),.001)
save('Quarry_Dawn_Underscore',music)
def noise(t):
    n=rng.normal(0,1,len(t)); return np.convolve(n,np.ones(7)/7,mode='same')
t=np.arange(SR*4)/SR
# Exact integer-cycle loops; gentle rotating machinery, no heavy DSP filters at runtime.
save('Refinery_Loop',.18*np.sin(2*np.pi*72*t)*(1+.2*np.sin(2*np.pi*3*t))+.07*np.sin(2*np.pi*144*t)+.03*np.sin(2*np.pi*431*t))
save('Truck_Engine_Loop',.20*np.sin(2*np.pi*48*t)*(1+.25*np.sin(2*np.pi*6*t))+.08*np.sin(2*np.pi*96*t)+.03*np.sin(2*np.pi*173*t))
t=np.arange(int(SR*.8))/SR
save('Stone_Break',np.tanh(noise(t)*2)*np.exp(-t*6)*.55+np.sin(2*np.pi*(85*t-22*t*t))*np.exp(-t*16)*.18)
t=np.arange(int(SR*.32))/SR
save('Ore_Pickup',(.22*np.sin(2*np.pi*440*t)+.09*np.sin(2*np.pi*660*t))*np.exp(-t*18)*(1-np.exp(-t*300)))
save('Ore_Place',noise(t)*np.exp(-t*20)*.35+np.sin(2*np.pi*170*t)*np.exp(-t*30)*.28)
t=np.arange(int(SR*.65))/SR
save('Refinery_Ready',(.16*np.sin(2*np.pi*523.25*t)+.12*np.sin(2*np.pi*659.25*t))*np.exp(-t*8)*(1-np.exp(-t*80)))
t=np.arange(int(SR*.9))/SR
save('Delivery_Confirm',sum(.13*np.sin(2*np.pi*f*t)*np.exp(-np.maximum(0,t-o)*8)*(t>=o)*(1-np.exp(-np.maximum(0,t-o)*80)) for f,o in [(293.66,0),(440,.12),(587.33,.24)]))
for p in ROOT.glob('*.wav'):
    with wave.open(str(p),'rb') as f: print(p.name,round(f.getnframes()/f.getframerate(),2),'s',f.getnchannels(),'channels')
