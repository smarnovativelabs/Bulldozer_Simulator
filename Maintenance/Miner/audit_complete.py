import bpy, os, json
from mathutils import Vector
paths=[r'C:/Users/Administrator/Downloads/construction-worker/source/complete.fbx']
reports=[]
for path in paths:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.fbx(filepath=os.path.abspath(path))
 report={'file':path,'meshes':[],'rigs':[],'actions':[],'images':[]}
 for o in bpy.data.objects:
  if o.type=='MESH':
   o.data.calc_loop_triangles();pts=[o.matrix_world @ v.co for v in o.data.vertices]
   report['meshes'].append({'name':o.name,'triangles':len(o.data.loop_triangles),'vertices':len(o.data.vertices),'materials':[m.name if m else None for m in o.data.materials],'bounds':[(min(p[i] for p in pts),max(p[i] for p in pts)) for i in range(3)],'groups':len(o.vertex_groups)})
  if o.type=='ARMATURE':report['rigs'].append({'name':o.name,'bones':len(o.data.bones),'names':[b.name for b in o.data.bones][:12]})
 report['actions']=[{'name':a.name,'range':list(a.frame_range)} for a in bpy.data.actions]
 report['images']=[{'name':i.name,'size':list(i.size),'packed':bool(i.packed_file),'path':i.filepath} for i in bpy.data.images]
 reports.append(report)
 if 'Breathing' in path:
  bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/Miner/BreathingSource.blend'))
open('Maintenance/Miner/complete-audit.json','w').write(json.dumps(reports,indent=2))
print(json.dumps(reports,indent=2))

