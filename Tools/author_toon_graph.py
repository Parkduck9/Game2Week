"""설치된 Shader Graph 템플릿에서 공용 툰 그래프를 재현한다."""
import copy
import json
from pathlib import Path
import uuid

ROOT = Path(__file__).resolve().parents[1]
PACKAGES = ROOT / 'Library/PackageCache'

def objects(path):
    source = path.read_text(encoding='utf-8-sig')
    decoder = json.JSONDecoder()
    index, result = 0, []
    while index < len(source):
        while index < len(source) and source[index].isspace():
            index += 1
        if index == len(source):
            break
        item, index = decoder.raw_decode(source, index)
        result.append(item)
    return result

def build():
    package = next(PACKAGES.glob('com.unity.shadergraph@*'))
    original = objects(package / 'GraphTemplates/Cross Pipeline/Unlit Simple.shadergraph')
    graph = original[0]
    mapping = {item['m_ObjectId']: item for item in original}
    find = lambda suffix: next(item for item in original if item['m_Type'].endswith(suffix))
    color = find('ColorShaderProperty')
    color['m_Name'] = '기본 색'; color['isMainColor'] = True
    color_node = next(item for item in original if item['m_Type'].endswith('PropertyNode') and item['m_Property']['m_Id'] == color['m_ObjectId'])
    block_nodes = [mapping[ref['m_Id']] for ref in graph['m_VertexContext']['m_Blocks'] + graph['m_FragmentContext']['m_Blocks']]
    retained = [graph, color, color_node, mapping[color_node['m_Slots'][0]['m_Id']]]
    for node in block_nodes:
        retained += [node] + [mapping[ref['m_Id']] for ref in node['m_Slots']]
    target = find('UniversalTarget'); target['m_RenderFace'] = 2
    retained += [target, find('UniversalUnlitSubTarget')]
    graph['m_Properties'] = [{'m_Id': color['m_ObjectId']}]
    graph['m_ActiveTargets'] = [{'m_Id': target['m_ObjectId']}]
    graph['m_CategoryData'] = []
    graph['m_Path'] = 'Game2Week'
    # 파일 GUID가 안정적으로 유지되며 그래프는 사용자 편집이 가능한 일반 그래프이다.
    identifier = lambda label: uuid.uuid5(uuid.NAMESPACE_URL, 'Game2Week/툰/' + label).hex
    full = objects(package / 'GraphTemplates/Cross Pipeline/1_Lit Full.shadergraph')
    function = copy.deepcopy(next(item for item in full if item['m_Type'].endswith('CustomFunctionNode')))
    function.update(m_ObjectId=identifier('함수'), m_Group={'m_Id': ''}, m_Name='셀 명암과 림',
                    m_FunctionName='ToonLighting', m_SourceType=0,
                    m_FunctionSource='fea0360f5dd94b5c92a20d5f44c11f70', m_FunctionBody='')
    slots = []
    for index, (label, slot_type, output) in enumerate([
            ('BaseColor', 'Vector4MaterialSlot', False), ('Normal', 'Vector3MaterialSlot', False),
            ('View', 'Vector3MaterialSlot', False), ('Out', 'Vector3MaterialSlot', True)]):
        slot = dict(m_SGVersion=0, m_Type='UnityEditor.ShaderGraph.' + slot_type,
                    m_ObjectId=identifier(label), m_Id=index, m_DisplayName=label,
                    m_SlotType=int(output), m_Hidden=False, m_ShaderOutputName=label,
                    m_StageCapability=2, m_Value=dict(x=0, y=0, z=0, w=1),
                    m_DefaultValue=dict(x=0, y=0, z=0, w=1), m_Labels=[])
        if 'Normal' in slot_type or 'ViewDirection' in slot_type:
            slot['m_Space'] = 2
        slots.append(slot)
    function['m_Slots'] = [{'m_Id': slot['m_ObjectId']} for slot in slots]
    retained += [function] + slots
    geometry = []
    for label, kind, version in [('법선', 'NormalVectorNode', 0), ('시선', 'ViewDirectionNode', 1)]:
        node = copy.deepcopy(color_node)
        node.pop('m_Property', None)
        node.update(m_ObjectId=identifier(label), m_Type='UnityEditor.ShaderGraph.' + kind,
                    m_SGVersion=version, m_Name=label, m_Space=2)
        slot = copy.deepcopy(slots[3])
        slot.update(m_ObjectId=identifier(label + '출력'), m_Id=0, m_DisplayName='Out')
        node['m_Slots'] = [{'m_Id': slot['m_ObjectId']}]
        geometry.append(node)
        retained += [node, slot]
    graph['m_Nodes'] = [{'m_Id': node['m_ObjectId']} for node in block_nodes + [color_node, function] + geometry]
    edge = lambda a, sa, b, sb: dict(m_OutputSlot=dict(m_Node=dict(m_Id=a['m_ObjectId']), m_SlotId=sa), m_InputSlot=dict(m_Node=dict(m_Id=b['m_ObjectId']), m_SlotId=sb))
    base_block = next(node for node in block_nodes if node['m_Name'] == 'SurfaceDescription.BaseColor')
    graph['m_Edges'] = [edge(color_node, 0, function, 0), edge(function, 3, base_block, 0),
                        edge(geometry[0], 0, function, 1), edge(geometry[1], 0, function, 2)]
    folder = ROOT / 'Assets/_Project/Art/Shaders'
    folder.mkdir(parents=True, exist_ok=True)
    (folder / 'ToonCharacter.shadergraph').write_text('\n\n'.join(json.dumps(item, indent=4, ensure_ascii=False) for item in retained), encoding='utf-8')
    print('공용 툰 Shader Graph 생성 완료')

if __name__ == '__main__':
    build()
