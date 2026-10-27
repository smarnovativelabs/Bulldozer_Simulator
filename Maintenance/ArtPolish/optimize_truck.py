import bpy,os,json,math
from mathutils import Vector
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r'C:\Users\Administrator\Downloads\man-tga-dump-truck\man tga.fbx')
out=os.path.abspath('Assets/0_ProjectData/Player/Art/Truck');report=[]
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
for o in meshes:
 bpy.context.view_layer.objects.active=o;o.select_set(True)
 o.data.calc_loop_triangles();before=len(o.data.loop_triangles)
 target=2500 if 'body_main' in o.name else 160 if 'wheel' in o.name else 650 if o.name=='platform' else 100
 mod=o.modifiers.new('Mobile reduction','DECIMATE');mod.ratio=min(1,target/before);bpy.ops.object.modifier_apply(modifier=mod.name)
 o.data.calc_loop_triangles();report.append(dict(name=o.name,source=before,triangles=len(o.data.loop_triangles)))
 if o.data.uv_layers:o.data.uv_layers[0].name='SourceUV'
 for mat in o.data.materials:
  if not mat:continue
  mat.use_nodes=True;nodes=mat.node_tree.nodes;p=nodes.get('Principled BSDF')
  if p:
   for link in list(mat.node_tree.links):
    if link.to_socket==p.inputs['Alpha']:mat.node_tree.links.remove(link)
   p.inputs['Alpha'].default_value=1;p.inputs['Metallic'].default_value=0;p.inputs['Roughness'].default_value=.65
  for node in list(nodes):
   if node.type=='TEX_IMAGE' and node.image:
    node.image.filepath='C:/Users/Administrator/Downloads/man-tga-dump-truck/'+os.path.basename(node.image.filepath.replace('\\','/'));node.image.reload()
    uv=nodes.new('ShaderNodeUVMap');uv.uv_map='SourceUV';mat.node_tree.links.new(uv.outputs['UV'],node.inputs['Vector'])
 o.select_set(False)
bpy.ops.object.select_all(action='DESELECT')
for o in meshes:o.select_set(True)
bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();truck=bpy.context.object;truck.name='MAN_Mobile_Truck'
bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
# Imported source uses Y as its physical vertical axis.
# FBX importer already converted the source's physical up axis.
coords=[truck.matrix_world@v.co for v in truck.data.vertices];lo=Vector(tuple(min(v[a] for v in coords) for a in range(3)));hi=Vector(tuple(max(v[a] for v in coords) for a in range(3)))
center=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
for v in truck.data.vertices:v.co=truck.matrix_world@v.co-center
truck.matrix_world.identity()
truck.data.uv_layers.new(name='MobileAtlas');truck.data.uv_layers.active_index=1
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=.012);bpy.ops.object.mode_set(mode='OBJECT')
atlas=bpy.data.images.new('TruckAtlas',width=1024,height=1024,alpha=False)
for mat in truck.data.materials:
 node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=atlas;mat.node_tree.nodes.active=node
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=1;scene.render.bake.use_pass_direct=False;scene.render.bake.use_pass_indirect=False;scene.render.bake.use_pass_color=True;scene.render.bake.margin=8
bpy.ops.object.bake(type='DIFFUSE');atlas.filepath_raw=out+'/TruckAtlas.png';atlas.file_format='PNG';atlas.save()
mat=bpy.data.materials.new('TruckAtlas');mat.use_nodes=True;p=mat.node_tree.nodes.get('Principled BSDF');p.inputs['Roughness'].default_value=.65;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas;mat.node_tree.links.new(tex.outputs['Color'],p.inputs['Base Color'])
for poly in truck.data.polygons:poly.material_index=0
truck.data.materials.clear();truck.data.materials.append(mat);truck.data.uv_layers.remove(truck.data.uv_layers['SourceUV']);truck.data.calc_loop_triangles()
count=len(truck.data.loop_triangles);assert count<5000,count
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/ArtPolish/TruckMobile.blend'))
bpy.ops.export_scene.fbx(filepath=out+'/TruckMobile.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='AUTO')
open('Maintenance/ArtPolish/truck-optimization.json','w').write(json.dumps(dict(parts=report,triangles=count,materials=1,atlas=1024,dimensions=list(truck.dimensions)),indent=2))
bpy.ops.object.camera_add(location=(8,-10,6));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1.5))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=9;scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(3,-4,8));bpy.context.object.data.energy=1800;bpy.context.object.data.size=6
scene.world=bpy.data.worlds.new('World');scene.world.color=(.25,.25,.25);scene.cycles.samples=16;scene.render.resolution_x=900;scene.render.resolution_y=700;scene.render.resolution_percentage=100;scene.render.filepath=os.path.abspath('Maintenance/ArtPolish/truck-preview.png');bpy.ops.render.render(write_still=True)


