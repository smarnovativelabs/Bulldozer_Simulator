import bpy,os,json
from mathutils import Vector
from mathutils.kdtree import KDTree
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=r'C:\Users\Administrator\Downloads\man-tga-dump-truck\man tga.fbx')
points=[]; wheels={}
for o in [o for o in bpy.context.scene.objects if o.type=='MESH']:
 bpy.context.view_layer.objects.active=o
 o.data.calc_loop_triangles(); n=len(o.data.loop_triangles)
 target=2500 if 'body_main' in o.name else 160 if 'wheel' in o.name else 650 if o.name=='platform' else 100
 mod=o.modifiers.new('Mobile reduction','DECIMATE');mod.ratio=min(1,target/n);bpy.ops.object.modifier_apply(modifier=mod.name)
 coords=[o.matrix_world@v.co for v in o.data.vertices]
 group=o.name if 'wheel' in o.name else 'Chassis'
 points.extend((p,group) for p in coords)
 if group!='Chassis':
  lo=Vector(tuple(min(p[a] for p in coords) for a in range(3))); hi=Vector(tuple(max(p[a] for p in coords) for a in range(3)))
  wheels[group]=(lo+hi)/2
lo=Vector(tuple(min(p[a] for p,g in points) for a in range(3))); hi=Vector(tuple(max(p[a] for p,g in points) for a in range(3)))
center=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
kd=KDTree(len(points))
for i,(p,g) in enumerate(points):kd.insert(p-center,i)
kd.balance()
bpy.ops.wm.open_mainfile(filepath=os.path.abspath('Maintenance/ArtPolish/TruckMobile.blend'))
truck=next(o for o in bpy.context.scene.objects if o.type=='MESH')
for o in list(bpy.context.scene.objects):
 if o!=truck:bpy.data.objects.remove(o,do_unlink=True)
groups={name:truck.vertex_groups.new(name=name) for name in ['Chassis']+list(wheels)}
maxerror=0;counts={name:0 for name in groups}
for v in truck.data.vertices:
 co,index,dist=kd.find(v.co);maxerror=max(maxerror,dist);name=points[index][1];groups[name].add([v.index],1,'REPLACE');counts[name]+=1
assert maxerror<.001,maxerror
arm=bpy.data.armatures.new('TruckRig');rig=bpy.data.objects.new('TruckRig',arm);bpy.context.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True);truck.select_set(False);bpy.ops.object.mode_set(mode='EDIT')
root=arm.edit_bones.new('Chassis');root.head=(0,0,0);root.tail=(0,0,.25)
for name,p in wheels.items():
 bone=arm.edit_bones.new(name);bone.head=p-center;bone.tail=bone.head+Vector((0,0,.2));bone.parent=root
bpy.ops.object.mode_set(mode='OBJECT');mod=truck.modifiers.new('Rigid wheel skinning','ARMATURE');mod.object=rig;truck.parent=rig
bpy.ops.object.select_all(action='SELECT')
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Maintenance/ArtPolish/TruckDelivery.blend'))
bpy.ops.export_scene.fbx(filepath=os.path.abspath('Assets/0_ProjectData/Player/Art/Truck/TruckDelivery.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='AUTO')
truck.data.calc_loop_triangles()
open('Maintenance/ArtPolish/truck-rig-audit.json','w').write(json.dumps(dict(triangles=len(truck.data.loop_triangles),maxMappingError=maxerror,verticesByBone=counts,wheelCenters={k:list(v-center) for k,v in wheels.items()},materials=len(truck.data.materials)),indent=2))
