import bpy,os,math
from mathutils import Matrix
bpy.ops.wm.open_mainfile(filepath=os.path.abspath('Maintenance/Miner/MinerMobile.blend'))
rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
rig.data.pose_position='POSE';bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
b=rig.data.bones['mixamorig1:RightHand'];pb=rig.pose.bones[b.name]
deform=rig.matrix_world@pb.matrix@b.matrix_local.inverted()@rig.matrix_world.inverted()
hand=rig.matrix_world@pb.head
palm=(rig.matrix_world@rig.pose.bones['mixamorig1:RightHandMiddle1'].head-hand)*.72
turn=Matrix.Rotation(math.pi,4,'X')
o=bpy.data.objects['Pickaxe']
for v in o.data.vertices:
 world=deform@(o.matrix_world@v.co)
 v.co=o.matrix_world.inverted()@(deform.inverted()@(hand+turn@(world-hand)+palm))
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.data.objects:
 if o.type in ['MESH','ARMATURE']:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=os.path.abspath('Assets/0_ProjectData/Player/Art/Optimized/MinerMobile.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=1,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/Miner/MinerMobile.blend'))
