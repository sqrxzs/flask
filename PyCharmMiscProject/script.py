import matplotlib.pyplot as plt
import numpy as np



def find_intersection(line1, line2):

    a1, b1, c1 = line1
    a2, b2, c2 = line2
    det = a1 * b2 - a2 * b1
    if abs(det) < 1e-9:
        return None
    x = (c1 * b2 - c2 * b1) / det
    y = (a1 * c2 - a2 * c1) / det
    return x, y

def is_feasible(x, y, constraints):

    eps = 1e-9
    for a, b, c, sign in constraints:
        val = a * x + b * y
        if sign == -1:
            if val - c > eps:
                return False
        elif sign == 1:
            if c - val > eps:
                return False
        else:
            if abs(val - c) > eps:
                return False
    return True

def solve_lp_2d(c, constraints, var_names=('x1', 'x2')):
    lines = []
    for a, b, rhs, sign in constraints:
        lines.append((a, b, rhs))

    lines.append((1, 0, 0))
    lines.append((0, 1, 0))

    vertices = []
    for i in range(len(lines)):
        for j in range(i + 1, len(lines)):
            p = find_intersection(lines[i], lines[j])
            if p is not None:
                x, y = p
                if is_feasible(x, y, constraints):
                    if not any(abs(x - vx) < 1e-6 and abs(y - vy) < 1e-6 for vx, vy in vertices):
                        vertices.append((x, y))
    if not vertices:
        return None, None, None, []

    best_value = -float('inf')
    best_point = None
    for x, y in vertices:
        val = c[0] * x + c[1] * y
        if val > best_value:
            best_value = val
            best_point = (x, y)

    return best_point[0], best_point[1], best_value, vertices

print("=" * 50)
print("ЗАДАЧА 1: СУДА")
print("=" * 50)


c1 = (20, 10)
constraints1 = [
    (12000, 7000, 60000, -1),   # <=
    (250,   100,  700,   -1),   # <=
    (2000, 1000, 7500,   1),    # >=
]

x1_opt, x2_opt, val_opt, verts1 = solve_lp_2d(c1, constraints1)
print("Модель:")
print("  max Z = 20*x1 + 10*x2")
print("  при ограничениях:")
print("    12000*x1 + 7000*x2 <= 60000")
print("    250*x1 + 100*x2 <= 700")
print("    2000*x1 + 1000*x2 >= 7500")
print("    x1 >= 0, x2 >= 0")
print()
if x1_opt is not None:
    print(f"Оптимальное решение: x1 = {x1_opt:.2f}, x2 = {x2_opt:.2f}")
    print(f"Максимальный доход: {val_opt:.2f} млн руб.")
else:
    print("Область допустимых решений пуста или задача не имеет решения.")
print()

print("=" * 50)
print("ЗАДАЧА 2: ПИТАНИЕ")
print("=" * 50)

c2 = (-0.20, -0.24)
constraints2 = [
    (1, 1, 10, 1),   # >=
    (4, 2, 12, 1),   # >=
    (2, 2, 8,  1),   # >=
    (0, 1, 1,  1),   # >=
]

x1_opt, x2_opt, val_opt, verts2 = solve_lp_2d(c2, constraints2)
print("Модель:")
print("  min Z = 0.20*x1 + 0.24*x2")
print("  при ограничениях:")
print("    x1 + x2 >= 10      (жиры)")
print("    4*x1 + 2*x2 >= 12  (белки)")
print("    2*x1 + 2*x2 >= 8   (углеводы)")
print("    x2 >= 1            (витамины)")
print("    x1 >= 0, x2 >= 0")
print()
if x1_opt is not None:
    cost = -val_opt
    print(f"Оптимальное решение: x1 = {x1_opt:.2f} кг, x2 = {x2_opt:.2f} кг")
    print(f"Минимальная стоимость: {cost:.2f} руб.")
else:
    print("Область допустимых решений пуста или задача не имеет решения.")
print()


print("=" * 50)
print("ЗАДАЧА 3: ДЕТАЛИ")
print("=" * 50)

c3 = (1.1, 1.5)
constraints3 = [
    (1, 2, 5000, -1),
    (2, 4, 10000, -1),
    (5, 3, 10000, -1),
    (1, 0, 2500, -1),
    (0, 1, 2000, -1),
    (1, 1, 1500, 1),
]

x1_opt, x2_opt, val_opt, verts3 = solve_lp_2d(c3, constraints3)
print("Модель:")
print("  max Z = 1.1*x1 + 1.5*x2")
print("  при ограничениях:")
print("    x1 + 2*x2 <= 5000   (рабочее время)")
print("    2*x1 + 4*x2 <= 10000 (полимер)")
print("    5*x1 + 3*x2 <= 10000 (листовой)")
print("    x1 <= 2500          (мощность А)")
print("    x2 <= 2000          (мощность В)")
print("    x1 + x2 >= 1500     (общее кол-во)")
print("    x1 >= 0, x2 >= 0")
print()
if x1_opt is not None:
    print(f"Оптимальное решение: x1 = {x1_opt:.2f} шт, x2 = {x2_opt:.2f} шт")
    print(f"Максимальный доход: {val_opt:.2f} руб.")
else:
    print("Область допустимых решений пуста или задача не имеет решения.")
print()


print("=" * 50)
print("ПРАКТИЧЕСКАЯ РАБОТА 2: ГРАФИЧЕСКОЕ РЕШЕНИЕ")
print("=" * 50)
print("Система:")
print("  x1 + x2 <= 5")
print("  3*x1 - x2 <= 3")
print("  x1 >= 0, x2 >= 0")
print()

constraints_gr = [
    (1, 1, 5, -1),
    (3, -1, 3, -1),
]

_, _, _, vertices_gr = solve_lp_2d((0, 0), constraints_gr)
print("Угловые точки области:")
for i, (x, y) in enumerate(vertices_gr):
    print(f"  {i+1}: x1 = {x:.2f}, x2 = {y:.2f}")

try:
    x = np.linspace(-0.5, 6, 400)
    # Прямая x1 + x2 = 5  -> x2 = 5 - x1
    y1 = 5 - x
    y2 = 3*x - 3

    plt.figure(figsize=(8, 6))
    plt.plot(x, y1, label='x1 + x2 = 5')
    plt.plot(x, y2, label='3x1 - x2 = 3')
    plt.axhline(0, color='black', linewidth=0.5)
    plt.axvline(0, color='black', linewidth=0.5)


    if vertices_gr:
        poly_vertices = [(0,0), (1,0), (2,3), (0,5)]
        poly = plt.Polygon(poly_vertices, alpha=0.3, color='lightblue', label='Допустимая область')
        plt.gca().add_patch(poly)

    plt.xlim(-0.5, 6)
    plt.ylim(-0.5, 6)
    plt.xlabel('x1')
    plt.ylabel('x2')
    plt.legend()
    plt.grid(True)
    plt.title('Графическое решение системы неравенств')
    plt.show()
except ImportError:
    print("Для построения графика требуется библиотека matplotlib (не установлена).")
    print("График не построен, но угловые точки найдены.")

print("\nГрафическое решение: допустимая область — четырёхугольник с вершинами (0,0), (1,0), (2,3), (0,5).")
