using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;

namespace Bulldozer.UI
{
    // Persistent soundtrack and a fixed voice budget. Gameplay calls only after successful actions.
    public sealed class QuarryAudio : MonoBehaviour
    {
        public enum Cue { Hit, Break, Pickup, Place, Ready, Cart, Delivery, Round }
        public AudioClip music, stoneBreak, pickup, place, ready, cart, delivery, refineryLoop, truckLoop;
        public AudioClip[] hits, footsteps;
        [Range(0,1)] public float musicVolume=.12f;
        public static QuarryAudio Instance { get; private set; }
        public AudioSource MusicSource => musicSource;
        public int PlayedCues { get; private set; }
        public int VoiceCount => voices.Length;
        readonly AudioSource[] voices=new AudioSource[8];
        readonly AudioSource[] machines=new AudioSource[3];
        readonly float[] nextCue=new float[8];
        AudioSource musicSource;
        MinerController player;
        MinerEquipment equipment;
        MiningProduction production;
        bool worldPaused,appPaused;
        float stepDistance;
        int hitIndex,stepIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()=>Instance=null;
        void Awake()
        {
            if(Instance&&Instance!=this){enabled=false;return;}
            Instance=this;
            musicSource=Source("Quarry music",false);musicSource.clip=music;musicSource.loop=true;musicSource.volume=0;musicSource.priority=160;
            for(int i=0;i<voices.Length;i++)voices[i]=Source("World voice "+i,true);
            for(int i=0;i<machines.Length;i++){machines[i]=Source("Machine loop "+i,true);machines[i].loop=true;machines[i].priority=220;machines[i].clip=i==0?refineryLoop:truckLoop;}
            SceneManager.sceneLoaded+=SceneLoaded;
        }
        AudioSource Source(string label,bool spatial)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();
            source.playOnAwake=false;source.spatialBlend=spatial?1:0;source.dopplerLevel=0;source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=4;source.maxDistance=28;source.priority=80;return source;
        }
        void Start()=>BindScene();
        void SceneLoaded(Scene scene,LoadSceneMode mode)=>BindScene();
        void BindScene()
        {
            player=FindFirstObjectByType<MinerController>();equipment=player?player.visual.GetComponent<MinerEquipment>():null;production=FindFirstObjectByType<MiningProduction>();
            stepDistance=0;foreach(var voice in voices)if(voice)voice.Stop();foreach(var machine in machines)if(machine)machine.Stop();
            if(player&&musicSource.clip&&!musicSource.isPlaying&&!appPaused)musicSource.Play();
        }
        void Update()
        {
            bool paused=appPaused||Time.timeScale<=0||!player||(CommonUI.Instance&&CommonUI.Instance.BlocksGameplayInput);
            float target=player?(paused?musicVolume*.45f:musicVolume):0;
            musicSource.volume=Mathf.MoveTowards(musicSource.volume,target,Time.unscaledDeltaTime*.18f);
            if(paused!=worldPaused){worldPaused=paused;foreach(var voice in voices){if(paused)voice.Pause();else voice.UnPause();}}
            if(paused){foreach(var machine in machines)if(machine.isPlaying)machine.Stop();stepDistance=0;return;}
            if(player.Grounded&&!player.IsMining&&!player.InputBlocked&&player.Speed>.2f){stepDistance+=player.Speed*Time.deltaTime;if(stepDistance>.95f){stepDistance=0;if(footsteps!=null&&footsteps.Length>0)Emit(footsteps[stepIndex++%footsteps.Length],player.transform.position,.22f,false);}}else stepDistance=0;
            if(!production)return;
            Loop(0,equipment.RefineryInputCount>0&&equipment.RefineryOutputCount<100,production.inputStack.position,.13f,1);
            Truck(1,production.delivery);Truck(2,production.secondDelivery);
        }
        void Truck(int index,TruckDelivery truck)
        {
            if(!truck){Loop(index,false,Vector3.zero,0,1);return;}
            bool moving=truck.Phase==TruckDelivery.DeliveryPhase.Arriving||truck.Phase==TruckDelivery.DeliveryPhase.Departing;
            Loop(index,moving&&truck.body.enabled,truck.transform.position,.14f,truck.Phase==TruckDelivery.DeliveryPhase.Departing?1.08f:.92f);
        }
        void Loop(int i,bool active,Vector3 position,float volume,float pitch)
        {
            var source=machines[i];source.transform.position=position;source.volume=volume;source.pitch=pitch;
            if(active&&source.clip){if(!source.isPlaying)source.Play();}else if(source.isPlaying)source.Stop();
        }
        public static void Play(Cue cue,Vector3 position){if(Instance)Instance.PlayCue(cue,position);}
        void PlayCue(Cue cue,Vector3 position)
        {
            if(appPaused||Time.timeScale<=0||!player||!QuarryUIFeedback.Sound||(CommonUI.Instance&&CommonUI.Instance.BlocksGameplayInput))return;
            int i=(int)cue;if(Time.unscaledTime<nextCue[i])return;nextCue[i]=Time.unscaledTime+(cue==Cue.Pickup?.12f:.08f);
            AudioClip clip=null;float volume=.35f;
            switch(cue){case Cue.Hit:if(hits!=null&&hits.Length>0)clip=hits[hitIndex++%hits.Length];volume=.65f;break;case Cue.Break:clip=stoneBreak;volume=.55f;break;case Cue.Pickup:clip=pickup;volume=.24f;break;case Cue.Place:clip=place;volume=.4f;break;case Cue.Ready:clip=ready;volume=.22f;break;case Cue.Cart:clip=cart;volume=.35f;break;case Cue.Delivery:case Cue.Round:clip=delivery;volume=cue==Cue.Round?.5f:.35f;break;}
            Emit(clip,position,volume,cue==Cue.Delivery||cue==Cue.Round);
        }
        void Emit(AudioClip clip,Vector3 position,float volume,bool important)
        {
            if(!clip||!QuarryUIFeedback.Sound)return;AudioSource source=null;
            foreach(var voice in voices)if(!voice.isPlaying){source=voice;break;}
            if(!source){if(!important)return;source=voices[0];source.Stop();}
            source.transform.position=position;source.clip=clip;source.volume=volume;source.spatialBlend=important?0:1;source.pitch=1;source.Play();PlayedCues++;
        }
        void OnApplicationPause(bool paused){appPaused=paused;if(!musicSource)return;if(paused)musicSource.Pause();else musicSource.UnPause();}
        void OnDisable(){if(musicSource)musicSource.Stop();foreach(var source in voices)if(source)source.Stop();foreach(var source in machines)if(source)source.Stop();}
        void OnDestroy(){SceneManager.sceneLoaded-=SceneLoaded;if(Instance==this)Instance=null;}
    }
}
