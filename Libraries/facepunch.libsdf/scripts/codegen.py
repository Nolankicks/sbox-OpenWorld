output = []
output.append("//")
output.append("// PLEASE DO NOT EDIT!!")
output.append("// This file was generated by Scripts/codegen.py, go edit that instead!")
output.append("//")
output.append("")
output.append("namespace Sandbox.Sdf;")
output.append("")
output.append("partial class Sdf3DMeshWriter")
output.append("{")
output.append("\tpartial void AddTriangles( in Sdf3DArrayData data, int x, int y, int z )")
output.append("\t{")
output.append("\t\tvar aRaw = data[x + 0, y + 0, z + 0];")
output.append("\t\tvar bRaw = data[x + 1, y + 0, z + 0];")
output.append("\t\tvar cRaw = data[x + 0, y + 1, z + 0];")
output.append("\t\tvar dRaw = data[x + 1, y + 1, z + 0];")
output.append("")
output.append("\t\tvar eRaw = data[x + 0, y + 0, z + 1];")
output.append("\t\tvar fRaw = data[x + 1, y + 0, z + 1];")
output.append("\t\tvar gRaw = data[x + 0, y + 1, z + 1];")
output.append("\t\tvar hRaw = data[x + 1, y + 1, z + 1];")
output.append("")
output.append("\t\tvar a = aRaw < 128 ? CubeConfiguration.A : 0;")
output.append("\t\tvar b = bRaw < 128 ? CubeConfiguration.B : 0;")
output.append("\t\tvar c = cRaw < 128 ? CubeConfiguration.C : 0;")
output.append("\t\tvar d = dRaw < 128 ? CubeConfiguration.D : 0;")
output.append("")
output.append("\t\tvar e = eRaw < 128 ? CubeConfiguration.E : 0;")
output.append("\t\tvar f = fRaw < 128 ? CubeConfiguration.F : 0;")
output.append("\t\tvar g = gRaw < 128 ? CubeConfiguration.G : 0;")
output.append("\t\tvar h = hRaw < 128 ? CubeConfiguration.H : 0;")
output.append("")
output.append("\t\tvar config = a | b | c | d | e | f | g | h;")
output.append("")
output.append("\t\tswitch ( config )")
output.append("\t\t{")

A = 0
B = 1
C = 2
D = 3

E = 4
F = 5
G = 6
H = 7

faces: list[list[int]] = [
    [A, B, D, C],
    [E, G, H, F],
    [A, E, F, B],
    [A, C, G, E],
    [C, D, H, G],
    [B, F, H, D]
]

names = [chr(65 + i) for i in range(8)]

def sort_cut(cut: tuple[int, int]):
    if cut[0] > cut[1]:
        return (cut[1], cut[0])
    else:
        return cut

def cut_to_vertex(cut: tuple[int, int]):
    cut = sort_cut(cut)
    return f"CubeVertex.{names[cut[0]]}{names[cut[1]]}"

for i in range(256):
    solid = [(i & (1 << j)) != 0 for j in range(8)]

    cases = ""

    keys = [names[i] for i, x in enumerate(solid) if x]
    if len(keys) == 0:
        cases += "CubeConfiguration.None"
    else:
        for key in keys:
            if len(cases) > 0:
                cases += " | "
            cases += f"CubeConfiguration.{key}"
    output.append(f"\t\t\tcase {cases}:")

    edges: list[tuple[tuple[int, int], tuple[int, int]]] = []

    for face in faces:
        prev = face[3]
        cuts: list[tuple[int, int]] = []

        for next in face:
            if solid[prev] != solid[next]:
                cuts.append((prev, next))
            prev = next

        if len(cuts) == 0:
            continue

        if solid[cuts[0][1]]:
            cuts.append(cuts[0])
            cuts = cuts[1:]

        for j in range(len(cuts) // 2):
            cut_a = cuts[j * 2 + 0]
            cut_b = cuts[j * 2 + 1]

            edges.append((cut_a, cut_b))
            prev = cut_b

    first_face = True

    while len(edges) > 0:
        if not first_face:
            output.append("")

        first_face = False

        prev_edge = edges.pop()

        face_edges = [prev_edge]

        found = True
        while found:
            found = False

            for edge in edges:
                if edge[0][0] != prev_edge[1][1] or edge[0][1] != prev_edge[1][0]:
                    continue

                found = True
                edges.remove(edge)
                face_edges.append(edge)
                prev_edge = edge
                break

        for i in range(0, len(face_edges) - 2):
            cut_a = face_edges[0][0]
            cut_b, cut_c = face_edges[i + 1]
            output.append(f"\t\t\t\tAddTriangle( x, y, z, {cut_to_vertex(cut_a)}, {cut_to_vertex(cut_b)}, {cut_to_vertex(cut_c)} );")

    output.append("\t\t\t\treturn;")
    output.append("")

output.append("\t\t}")
output.append("\t}")
output.append("}")

with open("../Code/3D/Sdf3DMeshWriter.Generated.cs", "tw", newline="\r\n") as f:
    f.writelines(line + "\n" for line in output)
