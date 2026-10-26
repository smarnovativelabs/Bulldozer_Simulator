"""Original smoke-test motion, not production locomotion. Run in a separate factory-startup Blender."""
import bpy, math
from mathutils import Quaternion, Vector
from pathlib import Path
root=Path('/Users/smarnovativelabs/Desktop/Bulldozer_Simulator')
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Sprint1Foundation/Art/QuaterniusWorker/Worker.fbx'))
a=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
bpy.context.scene.render.fps=30

def rotate(name,axis,angle):
 b=a.pose.bones[name]
 b.rotation_mode='QUATERNION'
 local=b.bone.matrix_local.to_quaternion().inverted() @ Vector(axis)
 b.rotation_quaternion=Quaternion(local,angle)

for name,frames in [('Idle',60),('Walk',30),('Run',20),('Interact',45)]:
 a.animation_data_create(); action=bpy.data.actions.new('Sprint1_'+name);a.animation_data.action=action
 for f in range(1,frames+2):
  phase=2*math.pi*(f-1)/frames
  for b in a.pose.bones:
   b.rotation_mode='QUATERNION';b.rotation_quaternion=Quaternion();b.location=(0,0,0)
  for side,sign in [('L',1),('R',-1)]:
   rotate('UpperArm.'+side,(0,1,0),sign*1.32)
  if name in ('Walk','Run'):
   amp=.35 if name=='Walk' else .55
   for side,sign in [('L',1),('R',-1)]:
    swing=math.sin(phase)*sign
    rotate('UpperLeg.'+side,(1,0,0),amp*swing)
    rotate('LowerLeg.'+side,(1,0,0),-.45*max(0,-swing))
    b=a.pose.bones['UpperArm.'+side]
    axis=b.bone.matrix_local.to_quaternion().inverted()@Vector((1,0,0))
    b.rotation_quaternion=Quaternion(axis,-amp*swing*.6)@b.rotation_quaternion
  elif name=='Interact':
   rotate('UpperArm.R',(0,1,0),-1.32+.85*(.5-.5*math.cos(phase)))
   rotate('LowerArm.R',(0,0,1),-.5*(.5-.5*math.cos(phase)))
  rotate('Chest',(1,0,0),.015*math.sin(phase))
  for b in a.pose.bones:
   b.keyframe_insert(data_path='rotation_quaternion',frame=f,group=b.name)
 action.use_fake_user=True
 a.animation_data.action=None
bpy.context.scene.frame_start=1;bpy.context.scene.frame_end=61
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Planning/Sprint1-Foundation/Source/WorkerTest.blend'))
bpy.ops.object.select_all(action='DESELECT');a.select_set(True);bpy.context.view_layer.objects.active=a
out=root/'Assets/Sprint1Foundation/Art/QuaterniusWorker/WorkerTestMotions.fbx'
if out.exists():raise RuntimeError('Refusing to overwrite export')
bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,object_types={'ARMATURE'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y')
print('EXPORTED',out,[(x.name,tuple(x.frame_range)) for x in bpy.data.actions])
