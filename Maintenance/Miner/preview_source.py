import bpy, os
from mathutils import Vector
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.abspath('Assets/0_ProjectData/Breathing Idle.fbx'))
bpy.context.scene.frame_set(1)
for m in bpy.data.materials:
 if m.use_nodes:
  p=m.node_tree.nodes.get('Principled BSDF')
  if p:
   p.inputs['Roughness'].default_value=.75
   for l in list(m.node_tree.links):
    if l.to_socket==p.inputs['Alpha']:m.node_tree.links.remove(l)
   p.inputs['Alpha'].default_value=1
bpy.ops.object.camera_add(location=(3,-5,2.7));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.35;bpy.context.scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(2,-3,5));bpy.context.object.data.energy=450;bpy.context.object.data.size=4
bpy.context.scene.world=bpy.data.worlds.new('PreviewWorld');bpy.context.scene.world.color=(.3,.3,.3)
s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=12;s.render.resolution_x=700;s.render.resolution_y=700;s.render.resolution_percentage=100;s.render.filepath=os.path.abspath('Maintenance/Miner/breathing-original.png');bpy.ops.render.render(write_still=True)

