if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
var path=Bulldozer.Player.Editor.DepositVerification.Prepare();
var root=System.IO.Path.GetFullPath("Maintenance/Miner/SaveTests")+System.IO.Path.DirectorySeparatorChar;
if(!System.IO.Path.GetFullPath(path).StartsWith(root,System.StringComparison.OrdinalIgnoreCase))throw new System.Exception("Isolated test path required");
var s=Bulldozer.Player.Persistence.MiningSnapshot.Create(0,85);s.damageLevel=1;s.taskRocks=2;s.taskOreMask=1;s.taskRefined=6;s.taskDeliveries=2;s.taskClaimedMask=7;s.trucksCompleted=2;
var store=new Bulldozer.Player.Persistence.MiningProgressStore(path);if(!store.TryCommit(s))throw new System.Exception("Test seed failed");EditorApplication.isPlaying=true;return path;
