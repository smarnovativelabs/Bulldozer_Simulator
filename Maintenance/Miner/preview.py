import bpy, os
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath=os.path.abspath('Maintenance/Miner/Miner.blend'))
for m in bpy.data.materials:
 if not m.use_nodes:
  c=m.diffuse_color[:];m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=c
bpy.ops.object.camera_add(location=(3,-5,2.7));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.7;bpy.context.scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(2,-3,5));bpy.context.object.data.energy=450;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=4
bpy.context.scene.world=bpy.data.worlds.new('PreviewWorld');bpy.context.scene.world.color=(.3,.3,.3)
s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=16;s.render.resolution_x=700;s.render.resolution_y=700;s.render.resolution_percentage=100;s.render.filepath=os.path.abspath('Maintenance/Miner/miner-preview.png');bpy.ops.render.render(write_still=True)

