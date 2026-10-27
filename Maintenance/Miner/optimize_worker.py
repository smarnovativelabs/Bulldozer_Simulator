import bpy, os, json, math
from mathutils import Vector, Matrix
OUT=os.path.abspath('Assets/0_ProjectData/Player/Art/Optimized')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.abspath('Assets/0_ProjectData/Breathing Idle.fbx'))
rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
rig.data.pose_position='REST'
report={'source':'Breathing Idle.fbx','before':53738,'meshes':[]}
budgets={'Ch17_Body':3000,'Ch17_Boots':450,'Ch17_Helmet':500,'Ch17_Pants':1100,'Ch17_Shirt':650,'Ch17_Vest':850}
for o in list(bpy.data.objects):
 if o.type!='MESH':continue
 if o.name not in budgets:bpy.data.objects.remove(o,do_unlink=True);continue
 bpy.context.view_layer.objects.active=o;bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
 o.data.calc_loop_triangles();before=len(o.data.loop_triangles)
 mod=o.modifiers.new('MobileBudget','DECIMATE');mod.ratio=budgets[o.name]/before;mod.use_collapse_triangulate=True
 # Keep more topology around face/fingers while simplifying broad surfaces.
 if o.name=='Ch17_Body':
  group=o.vertex_groups.new(name='PreserveDetails')
  for v in o.data.vertices:
   p=o.matrix_world @ v.co
   group.add([v.index],1.0 if p.z>1.58 or abs(p.x)>.72 else .15,'REPLACE')
  mod.vertex_group=group.name;mod.vertex_group_factor=.3
 bpy.ops.object.modifier_apply(modifier=mod.name)
 o.data.calc_loop_triangles();report['meshes'].append({'name':o.name,'before':before,'after':len(o.data.loop_triangles)})
# Export only the body diffuse map, at mobile-friendly resolution; drop redundant 4k maps.
tex=next(i for i in bpy.data.images if '1001_Diffuse' in i.filepath);tex.scale(2048,2048);tex.filepath_raw=os.path.join(OUT,'WorkerAlbedo.png');tex.file_format='PNG';tex.save()
mat=bpy.data.materials.new('WorkerAlbedo');mat.use_nodes=True;p=mat.node_tree.nodes.get('Principled BSDF');p.inputs['Roughness'].default_value=.8
n=mat.node_tree.nodes.new('ShaderNodeTexImage');n.image=tex;mat.node_tree.links.new(n.outputs['Color'],p.inputs['Base Color'])
hairtex=next(i for i in bpy.data.images if '1002_Diffuse' in i.filepath);hairtex.scale(1024,1024);hairtex.filepath_raw=os.path.join(OUT,'WorkerHair.png');hairtex.file_format='PNG';hairtex.save()
hairmat=bpy.data.materials.new('WorkerHair');hairmat.use_nodes=True;hp=hairmat.node_tree.nodes.get('Principled BSDF');hn=hairmat.node_tree.nodes.new('ShaderNodeTexImage');hn.image=hairtex;hairmat.node_tree.links.new(hn.outputs['Color'],hp.inputs['Base Color']);hairmat.node_tree.links.new(hn.outputs['Alpha'],hp.inputs['Alpha'])
for o in bpy.data.objects:
 if o.type=='MESH':o.data.materials.clear();o.data.materials.append(hairmat if o.name=='Ch17_Hair' else mat)
def material(name,c):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*c,1);return m
canvas=material('Canvas',(.26,.15,.07));steel=material('ForgedSteel',(.21,.26,.29));wood=material('Hickory',(.34,.18,.075));dark=material('Straps',(.055,.065,.065));ore=material('OreLoad',(.29,.33,.32));lamp=material('LampGlass',(.8,.92,1))
gear=[]
def bind(o,bone):
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 if bone=='RightHand':
  for v in o.data.vertices: v.co=hand_inverse @ (idle_hand+Matrix.Rotation(math.pi,4,'X')@(v.co-base))
 g=o.vertex_groups.new(name='mixamorig1:'+bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE');m=o.modifiers.new('Rig','ARMATURE');m.object=rig;w=o.matrix_world.copy();o.parent=rig;o.matrix_world=w;gear.append(o)
def box(name,p,s,ma,bone):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 b=o.modifiers.new('Edges','BEVEL');b.width=.015;b.segments=1;bpy.ops.object.modifier_apply(modifier=b.name);o.data.materials.append(ma);bind(o,bone);return o
for x in [-.20,.20]:box('RackUpright',(x,.26,1.22),(.045,.045,.65),wood,'Spine2')
for z in [1.0,1.2,1.4]:box('RackCrossbar',(0,.26,z),(.44,.045,.04),wood,'Spine2')
for x in [-.18,-.09,0,.09,.18]:box('RackShelf',(x,.43,.94),(.085,.40,.035),wood,'Spine2')
for x in [-.13,.13]:box('Harness',(x,.045,1.31),(.035,.34,.37),dark,'Spine2')
box('HeadLamp',(0,-.15,1.735),(.065,.045,.05),lamp,'Head')
# Hand-centred grip and tapered, swept steel head. Original mesh, not copied from a store model.
rig.data.pose_position='POSE';bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
b=rig.data.bones['mixamorig1:RightHand'];pb=rig.pose.bones[b.name]
hand_inverse=(rig.matrix_world @ pb.matrix @ b.matrix_local.inverted() @ rig.matrix_world.inverted()).inverted()
idle_hand=rig.matrix_world @ pb.head
idle_hand+=(rig.matrix_world @ rig.pose.bones['mixamorig1:RightHandMiddle1'].head-idle_hand)*.72
h=rig.matrix_world @ b.head_local
rig.data.pose_position='REST'
# In the T pose, a handle across the palm follows forearm roll into the idle pose.
base=Vector((h.x,h.y-.035,h.z))
verts=[];faces=[]
for z,r in [(-.48,.018),(-.36,.023),(.0,.019),(.34,.021)]:
 for j in range(8):
  ang=j*math.tau/8;verts.append(tuple(base+Vector((r*math.cos(ang),r*math.sin(ang),z))))
for k in range(3):
 for j in range(8):faces.append((k*8+j,k*8+(j+1)%8,(k+1)*8+(j+1)%8,(k+1)*8+j))
faces += [tuple(range(7,-1,-1)),tuple(range(24,32))]
mesh=bpy.data.meshes.new('PickaxeHandle');mesh.from_pydata(verts,[],faces);mesh.materials.append(wood);o=bpy.data.objects.new('PickaxeHandle',mesh);bpy.context.collection.objects.link(o);bind(o,'RightHand')
verts=[];faces=[]
for x,z,w,t in [(-.31,.23,.004,.007),(-.19,.30,.027,.028),(0,.35,.045,.045),(.17,.30,.03,.025),(.28,.23,.045,.008)]:
 for y,dz in [(-w,-t),(w,-t),(w,t),(-w,t)]:verts.append(tuple(base+Vector((x,y,z+dz))))
for k in range(4):
 for j in range(4):faces.append((k*4+j,k*4+(j+1)%4,(k+1)*4+(j+1)%4,(k+1)*4+j))
faces += [(3,2,1,0),(16,17,18,19)]
mesh=bpy.data.meshes.new('ForgedPickHead');mesh.from_pydata(verts,[],faces);mesh.materials.append(steel);o=bpy.data.objects.new('ForgedPickHead',mesh);bpy.context.collection.objects.link(o);bind(o,'RightHand')
# Share one palette material across equipment to reduce material draws.
palette=bpy.data.images.new('EquipmentPalette',width=64,height=64)
cs=[canvas.diffuse_color[:],steel.diffuse_color[:],wood.diffuse_color[:],dark.diffuse_color[:],ore.diffuse_color[:],lamp.diffuse_color[:]]
px=[]
for y in range(64):
 for x in range(64):px.extend(cs[min(5,y//10)])
palette.pixels=px;palette.filepath_raw=os.path.join(OUT,'EquipmentPalette.png');palette.file_format='PNG';palette.save()
pm=material('EquipmentPalette',(1,1,1));pn=pm.node_tree.nodes.new('ShaderNodeTexImage');pn.image=palette;pm.node_tree.links.new(pn.outputs['Color'],pm.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
for o in gear:
 idx=[canvas,steel,wood,dark,ore,lamp].index(o.data.materials[0])
 uv=o.data.uv_layers.active or o.data.uv_layers.new()
 for loop in uv.data:loop.uv=(.5,(idx*10+5)/64)
 o.data.materials.clear();o.data.materials.append(pm)
# Consolidate body + equipment into one skinned renderer, preserving UVs and weights.
bpy.ops.object.select_all(action='DESELECT')
meshes=[o for o in bpy.data.objects if o.type=='MESH' and o not in gear]
for o in meshes:
 o.select_set(True)
 if o.data.uv_layers.active:o.data.uv_layers.active.name='map1'
bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();body=bpy.context.object;body.name='MinerMobile';body.data.calc_loop_triangles()
report['body_triangles']=len(body.data.loop_triangles)
report['total_with_equipment']=sum(len(o.data.loop_triangles) for o in [body])
for o in gear:
 o.data.calc_loop_triangles();report['total_with_equipment']+=len(o.data.loop_triangles)
assert report['total_with_equipment']<8000,report
# Keep one Armature modifier after joining.
mods=[m for m in body.modifiers if m.type=='ARMATURE']
for m in mods[1:]:body.modifiers.remove(m)
bpy.ops.object.select_all(action='DESELECT');body.select_set(True);rig.select_set(True)
for o in gear:
 o.select_set(True)
 if o.data.uv_layers.active:o.data.uv_layers.active.name='map1'
rig.data.pose_position='POSE'
s=bpy.context.scene;s.frame_start=1;s.frame_end=299;s.frame_set(1)
if rig.animation_data and rig.animation_data.action:rig.animation_data.action.name='BreathingIdle'
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'MinerMobile.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=1,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
# Remove orphaned full-resolution embedded source images from editable optimized file.
for image in list(bpy.data.images):
 if image not in [tex,hairtex,palette]:bpy.data.images.remove(image)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/Miner/MinerMobile.blend'))
open('Maintenance/Miner/optimization-report.json','w').write(json.dumps(report,indent=2));print(json.dumps(report))



