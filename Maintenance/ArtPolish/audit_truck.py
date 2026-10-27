import bpy,json,os
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r'C:\Users\Administrator\Downloads\man-tga-dump-truck\man tga.fbx')
report=[]
for o in bpy.context.scene.objects:
 if o.type=='MESH':
  o.data.calc_loop_triangles()
  report.append(dict(name=o.name,triangles=len(o.data.loop_triangles),dimensions=list(o.dimensions),materials=[m.name if m else None for m in o.data.materials]))
images=[dict(name=i.name,path=i.filepath) for i in bpy.data.images]
open('Maintenance/ArtPolish/truck-source-audit.json','w').write(json.dumps(dict(objects=report,images=images),indent=2))
