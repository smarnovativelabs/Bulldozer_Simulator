import bpy,os
bpy.ops.wm.open_mainfile(filepath=os.path.abspath('Maintenance/Miner/MinerMobile.blend'))
rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
rig.data.pose_position='REST'
for name,prefixes in [('Pickaxe',['PickaxeHandle','ForgedPickHead']),('Rack',['Rack','Harness'])]:
 objects=[o for o in bpy.data.objects if o.type=='MESH' and any(o.name.startswith(p) for p in prefixes)]
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 for m in list(o.modifiers)[1:]:
  if m.type=='ARMATURE':o.modifiers.remove(m)
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.data.objects:
 if o.type in ['MESH','ARMATURE']:o.select_set(True)
rig.data.pose_position='POSE';bpy.context.scene.frame_set(1)
bpy.ops.export_scene.fbx(filepath=os.path.abspath('Assets/0_ProjectData/Player/Art/Optimized/MinerMobile.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=1,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/Miner/MinerMobile.blend'))
