using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
namespace Bulldozer.Player
{
    // Read-only guide: never mutates inventory, moves the player or blocks controls.
    public sealed class QuarryGuidance : MonoBehaviour
    {
        public enum Goal { Mine, PickUp, Refine, Collect, Load, Upgrade, Finished }
        public MinerController player;
        public MinerEquipment equipment;
        public MiningProduction production;
        public MinerUpgradeArea upgrades;
        public RectTransform dialog;
        public CanvasGroup dialogGroup;
        public TMP_Text heading,message,stepLabel,completionLabel;
        public UnityEngine.UI.Image accent,progress;
        public MeshFilter pathFilter;
        public MeshRenderer pathRenderer;
        public Material pathMaterial;
        public int agentTypeId;
        public Goal CurrentGoal {get;private set;}
        public Vector3 Destination {get;private set;}
        public bool HasRoute {get;private set;}
        public int PathTriangles=>mesh?mesh.triangles.Length/3:0;
        NavMeshPath route;
        readonly Vector3[] corners=new Vector3[64];
        readonly List<Vector3> vertices=new List<Vector3>(512);
        readonly List<Color> colors=new List<Color>(512);
        readonly List<int> indices=new List<int>(768);
        MineableRock[] rocks;NavMeshObstacle[] rockObstacles;
        Mesh mesh;Vector2 restingPosition;float evaluateAt,drawAt,changedAt,celebrateUntil;int count,lastDeliveries,lastLevel;bool initialized;
        string currentBody;Color tint=new Color(.35f,1,.7f);
        void Start()
        {
            route=new NavMeshPath();
            rocks=FindObjectsByType<MineableRock>(FindObjectsSortMode.None);rockObstacles=new NavMeshObstacle[rocks.Length];
            for(int i=0;i<rocks.Length;i++)rockObstacles[i]=rocks[i].GetComponent<NavMeshObstacle>();
            mesh=new Mesh{name="Guidance pooled arrow mesh"};mesh.MarkDynamic();pathFilter.sharedMesh=mesh;pathRenderer.sharedMaterial=pathMaterial;
            restingPosition=dialog.anchoredPosition;lastDeliveries=equipment.TrucksCompleted;lastLevel=equipment.DamageLevel+equipment.CapacityLevel;
            dialogGroup.blocksRaycasts=false;dialogGroup.interactable=false;RefreshObjective();
        }
        void Update()
        {
            if(Time.unscaledTime>=evaluateAt){evaluateAt=Time.unscaledTime+.4f;RefreshObjective();}
            float age=Time.unscaledTime-changedAt,t=Mathf.Clamp01(age/.35f),ease=1-Mathf.Pow(1-t,3);
            dialog.anchoredPosition=restingPosition+Vector2.up*(1-ease)*28;dialogGroup.alpha=ease;
            dialog.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*.035f);
            message.maxVisibleCharacters=Mathf.CeilToInt(Mathf.Max(0,age-.12f)*65);
            completionLabel.gameObject.SetActive(Time.unscaledTime<celebrateUntil);
            if(completionLabel.gameObject.activeSelf)completionLabel.color=new Color(.5f,1,.7f,Mathf.Clamp01((celebrateUntil-Time.unscaledTime)*2));
            if(Time.unscaledTime>=drawAt){drawAt=Time.unscaledTime+.05f;DrawPath();}
        }
        public void RefreshObjective()
        {
            if(!equipment||rocks==null)return;
            for(int i=0;i<rocks.Length;i++)if(rocks[i]&&rockObstacles[i])rockObstacles[i].enabled=!rocks[i].Depleted;
            var s=equipment.Snapshot;int raw=0,refined=0;foreach(int id in s.items){if(MiningResources.Refined(id))refined++;else raw++;}
            Goal goal;Vector3 target;string title,body;int step;
            if(refined>0){goal=Goal.Load;target=production.sellZone.transform.position;title="SHIP IT!";body="Walk onto the loading pad. Your stone will stack here, then load onto a truck automatically.";step=4;}
            else if(raw>0&&(equipment.IsFull||s.groundDrops.Length==0)){goal=Goal.Refine;target=production.inputZone.transform.position;title="FEED THE REFINERY";body="Stand inside the input outline. Put your stones down and watch them feed into the machine.";step=2;}
            else if(s.groundDrops.Length>0&&!equipment.IsFull){goal=Goal.PickUp;target=s.groundDrops[0].position;foreach(var drop in s.groundDrops)if((drop.position-player.transform.position).sqrMagnitude<(target-player.transform.position).sqrMagnitude)target=drop.position;title="PICK UP YOUR STONES";body="Walk over the loose stones to collect them. Let's fill that rack!";step=1;}
            else if(raw>0){goal=Goal.Refine;target=production.inputZone.transform.position;title="FEED THE REFINERY";body="Follow the arrows to the input pad. Unloading happens automatically.";step=2;}
            else if(s.refineryOutput.Length>0){goal=Goal.Collect;target=production.outputZone.transform.position;title="YOUR STONE IS READY";body="Step onto COLLECT to pick up the finished stone. Next stop: the trucks!";step=3;}
            else if(upgrades&&s.trucksCompleted>0&&s.damageLevel<3&&s.coins>=equipment.DamageCost){goal=Goal.Upgrade;target=upgrades.zone.transform.position;title="PUT THAT CASH TO WORK";body="Visit the workbench and upgrade your pickaxe for tougher rocks. Break rocks with fewer swings!";step=5;}
            else
            {
                goal=Goal.Mine;target=NearestRock();step=1;
                title=s.refineryInput.Length>0?"KEEP THE QUARRY MOVING":"LET'S BREAK SOME ROCKS";
                body=s.refineryInput.Length>0?"The refinery is working. Follow the arrows and mine more stone while it runs.":s.loadingQueue.Length>0?"Your delivery is being handled. Mine another batch while the trucks work.":"Follow the arrows. Stop beside the marked rock and your miner will swing automatically.";
                if(target==Vector3.positiveInfinity){goal=Goal.Finished;target=player.transform.position;title="QUARRY CLEARED!";body=s.refineryInput.Length>0?"Your last stones are refining. Collect them when they're ready.":"Great work! Finish your deliveries and spend your cash at the workbench.";step=5;}
            }
            if(equipment.UsingWheelbarrow&&(goal==Goal.Mine||goal==Goal.Upgrade)){title="PARK TO USE YOUR PICKAXE";body="Tap PARK CART to leave the wheelbarrow. Its cargo stays safe while you mine on foot.";target=player.transform.position;}
            bool changed=!initialized||goal!=CurrentGoal||body!=currentBody;
            if(initialized&&((goal==Goal.PickUp&&CurrentGoal==Goal.Mine)||(goal==Goal.Refine&&CurrentGoal==Goal.PickUp)||(goal==Goal.Collect&&CurrentGoal==Goal.Refine)||(goal==Goal.Mine&&CurrentGoal==Goal.Load)))Celebrate("NICE WORK! NEXT STEP");
            if(s.trucksCompleted>lastDeliveries)Celebrate("DELIVERY PAID!  CASH +");
            if(s.damageLevel+s.capacityLevel>lastLevel)Celebrate("UPGRADE COMPLETE!");
            lastDeliveries=s.trucksCompleted;lastLevel=s.damageLevel+s.capacityLevel;
            CurrentGoal=goal;Destination=target;currentBody=body;
            tint=goal==Goal.Load||goal==Goal.Upgrade?new Color(.45f,1,.6f):new Color(1,.77f,.2f);
            if(changed){changedAt=Time.unscaledTime;heading.text=title;message.text=body;stepLabel.text="QUARRY GUIDE  /  "+step+" OF 5";progress.fillAmount=step/5f;accent.color=progress.color=tint;initialized=true;}
            CalculateRoute();
        }
        void Celebrate(string text){completionLabel.text=text;celebrateUntil=Time.unscaledTime+2.2f;}
        Vector3 NearestRock()
        {
            float best=float.PositiveInfinity;Vector3 chosen=Vector3.positiveInfinity;
            var filter=new NavMeshQueryFilter{agentTypeID=agentTypeId,areaMask=NavMesh.AllAreas};
            foreach(var rock in rocks)
            {
                if(!rock||rock.Depleted||!rock.HitCollider)continue;
                var b=rock.HitCollider.bounds;
                for(int side=0;side<4;side++)
                {
                    Vector3 dir=side==0?Vector3.back:side==1?Vector3.left:side==2?Vector3.right:Vector3.forward;
                    Vector3 candidate=b.center+Vector3.Scale(dir,b.extents)+dir*.65f;candidate.y=0;
                    float distance=(candidate-player.transform.position).sqrMagnitude;if(distance>=best)continue;
                    if(NavMesh.SamplePosition(candidate,out var hit,.35f,filter)&&Mathf.Abs(hit.position.y-player.transform.position.y)<.8f){best=distance;chosen=hit.position;}
                }
            }
            return chosen;
        }
        void CalculateRoute()
        {
            HasRoute=false;count=0;if(CurrentGoal==Goal.Finished)return;
            var filter=new NavMeshQueryFilter{agentTypeID=agentTypeId,areaMask=NavMesh.AllAreas};
            if(!NavMesh.SamplePosition(player.transform.position,out var from,1.5f,filter)||!NavMesh.SamplePosition(Destination,out var to,.85f,filter))return;
            if(!NavMesh.CalculatePath(from.position,to.position,filter,route)||route.status!=NavMeshPathStatus.PathComplete)return;
            count=route.GetCornersNonAlloc(corners);HasRoute=count>0;Destination=to.position;
        }
        void DrawPath()
        {
            vertices.Clear();colors.Clear();indices.Clear();
            if(HasRoute&&!player.InputBlocked)
            {
                float offset=.55f+Mathf.Repeat(Time.unscaledTime*.8f,.7f);int arrows=0;
                for(int i=1;i<count&&arrows<48;i++)
                {
                    Vector3 a=corners[i-1],b=corners[i],delta=b-a;delta.y=0;float length=delta.magnitude;if(length<.01f)continue;Vector3 forward=delta/length,right=Vector3.Cross(Vector3.up,forward);
                    while(offset<length&&arrows<48){Vector3 c=a+forward*offset;c.y=.07f;Arrow(c,forward,right,tint);offset+=.7f;arrows++;}offset-=length;
                }
                float radius=.48f+Mathf.Sin(Time.unscaledTime*4)*.06f;
                for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Vector3 c=new Vector3(Destination.x,.075f,Destination.z);Vector3 d1=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),d2=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));Quad(c+d1*radius,c+d2*radius,c+d2*(radius+.07f),c+d1*(radius+.07f),tint);}
            }
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
        }
        void Arrow(Vector3 c,Vector3 f,Vector3 r,Color color)
        {
            // Two quads form one chevron; all arrows and the destination ring share a draw call.
            Quad(c-r*.22f-f*.18f,c-r*.22f-f*.07f,c+f*.16f,c+f*.05f,color);
            Quad(c+f*.05f,c+f*.16f,c+r*.22f-f*.07f,c+r*.22f-f*.18f,color);
        }
        void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color)
        {
            int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);for(int i=0;i<4;i++)colors.Add(color);
            indices.Add(n);indices.Add(n+1);indices.Add(n+2);indices.Add(n);indices.Add(n+2);indices.Add(n+3);
        }
        void OnDisable(){if(mesh)mesh.Clear();if(dialogGroup)dialogGroup.alpha=0;}
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
}


