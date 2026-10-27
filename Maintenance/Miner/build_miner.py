import bpy, math, os, json
from mathutils import Vector
ROOT=os.path.abspath('Assets/0_ProjectData/Player/Art')
SOURCE=os.path.abspath('../I_Am_Janitor/Assets/0_Project Data/3_Arts/FBX/NPC Characters/Man 1')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.join(SOURCE,'Man 1-1.fbx'))
rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
body=next(o for o in bpy.data.objects if o.type=='MESH')
body.name='MinerBody'
# Preserve face and hands from the source atlas; clothing gets its own replaceable atlas swatches.
original=bpy.data.images.load(os.path.join(SOURCE,'Man 1 Atlas.png'))
original.scale(512,512)
src=list(original.pixels)
colors=[(.10,.20,.25,1),(.95,.39,.045,1),(.85,.91,.79,1),(.075,.065,.055,1),(.99,.64,.07,1),(.20,.25,.23,1),(.34,.19,.085,1),(.28,.34,.36,1)]
pixels=[]
for y in range(512):
 for x in range(1024):
  z=y/512*2
  c=colors[3] if z<.19 else colors[0] if z<.97 else colors[2] if (1.10<z<1.14 or 1.37<z<1.41) else colors[1]
  pixels.extend(src[(y*512+x)*4:(y*512+x)*4+4] if x<512 else c if x<900 else colors[0])
tex=bpy.data.images.new('MinerAtlas',width=1024,height=512)
tex.pixels=pixels; tex.filepath_raw=os.path.join(ROOT,'MinerAtlas.png'); tex.file_format='PNG';tex.save()
mat=bpy.data.materials.new('MinerAtlas');mat.use_nodes=True
node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=tex
mat.node_tree.links.new(node.outputs['Color'],mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
body.data.materials.clear();body.data.materials.append(mat)
uv=body.data.uv_layers.active.data
for poly in body.data.polygons:
 p=sum((body.matrix_world @ body.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
 sw=None
 if abs(p.x)>.50: sw=None
 elif p.z<.19: sw=3
 elif p.z<.97: sw=0
 elif p.z<1.56 and abs(p.x)<.34:
  sw=1
 elif p.z<1.48 and abs(p.x)<.52: sw=0
 for li in poly.loop_indices:
  if sw is None: uv[li].uv.x*=.5
  else:
   v=body.matrix_world @ body.data.vertices[body.data.loops[li].vertex_index].co
   uv[li].uv=(.95 if abs(p.x)>.34 and p.z>.97 else .70, max(.002,min(.998,v.z/2)))

def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*color,1);return m
helmet=material('SafetyYellow',(.98,.58,.035));dark=material('Charcoal',(.045,.065,.07));steel=material('Steel',(.27,.34,.38));leather=material('CanvasBrown',(.28,.15,.06));ore=material('Ore',(.23,.29,.31));lamp=material('Lamp',(.9,.97,1))
def cube(name,pos,scale,ma,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.scale=scale
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Soft edges','BEVEL');mod.width=bevel;mod.segments=2
  bpy.ops.object.modifier_apply(modifier=mod.name)
 o.data.materials.append(ma);return o

def sphere(name,pos,scale,ma):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=1,location=pos);o=bpy.context.object;o.name=name;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(ma);return o

def bind(o,bone):
 # Rigid bone weights preserve placement through FBX export and procedural locomotion.
 bpy.context.view_layer.objects.active=o;o.select_set(True)
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 g=o.vertex_groups.new(name='mixamorig:'+bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
 m=o.modifiers.new('EquipmentRig','ARMATURE');m.object=rig;world=o.matrix_world.copy();o.parent=rig;o.matrix_world=world

hat=[sphere('HardHat',(0,.025,1.795),(.145,.14,.095),helmet),sphere('HatBrim',(0,.012,1.755),(.18,.18,.018),helmet),cube('LampHousing',(0,-.118,1.79),(.085,.055,.07),dark),sphere('HeadLamp',(0,-.15,1.79),(.03,.012,.027),lamp)]
for o in hat: bind(o,'Head')
pack=[cube('OreBackpack',(0,.20,1.20),(.38,.25,.43),leather),cube('PackFrame',(0,.33,1.19),(.32,.025,.40),dark)]
for x in [-.13,.13]: pack.append(cube('BackpackStrap',(x,.065,1.28),(.045,.34,.39),dark,.012))
for i in range(7):
 x=((i%3)-1)*.105;y=.19+(i//3)*.065;z=1.42+(i%2)*.045
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.09,location=(x,y,z));o=bpy.context.object;o.name='OreLoad';o.data.materials.append(ore);pack.append(o)
# Export equipment separately as reusable FBXs before binding to character.
def export_selected(items,name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in items:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,name+'.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
export_selected(pack,'MinerBackpack')
for o in pack:bind(o,'Spine2')
# Pickaxe is held next to the right hand in the rest pose.
pick=[cube('PickaxeHandle',(-.606,.018,.91),(.035,.035,.64),leather,.008),cube('PickaxeHead',(-.606,.018,1.22),(.38,.065,.065),steel,.018)]
for sign in [-1,1]:
 o=cube('PickaxeTip',(-.606+sign*.22,.018,1.18),(.17,.05,.045),steel,.01);o.rotation_euler.y=sign*.48;pick.append(o)
export_selected(pick,'MinerPickaxe')
for o in pick:bind(o,'RightHand')
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'Miner.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',path_mode='COPY',embed_textures=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/Miner/Miner.blend'))
print('MINER_EXPORT_COMPLETE')
