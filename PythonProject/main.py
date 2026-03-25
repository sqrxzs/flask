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

def solve_lp_2d(c, constraints):
    lines = [(a, b, c) for a, b, c, _ in constraints]
    lines.append((1, 0, 0))
    lines.append((0, 1, 0))
    vertices = []
    for i in range(len(lines)):
        for j in range(i+1, len(lines)):
            p = find_intersection(lines[i], lines[j])
            if p:
                x, y = p
                if is_feasible(x, y, constraints):
                    if not any(abs(x-vx)<1e-6 and abs(y-vy)<1e-6 for vx, vy in vertices):
                        vertices.append((x, y))
    if not vertices:
        return None, None, None, []
    best_val = -float('inf')
    best_point = None
    for x, y in vertices:
        val = c[0]*x + c[1]*y
        if val > best_val:
            best_val = val
            best_point = (x, y)
    return best_point[0], best_point[1], best_val, vertices

print("ВАРИАНТ 2\n")

print("1. Графическое решение систем неравенств")
print("а) x1 + x2 ≤ 5, 3x1 - x2 ≤ 3, x1 ≥ 0, x2 ≥ 0")
cons_a = [(1, 1, 5, -1), (3, -1, 3, -1)]
_, _, _, verts_a = solve_lp_2d((0,0), cons_a)
print("Угловые точки области:")
for x, y in verts_a:
    print(f"  ({x:.2f}, {y:.2f})")

print("\nб) x1 - x2 ≤ 3, x1 + x2 ≤ 9, -x1 + x2 ≥ 3, x1 + x2 ≥ 1.5, x1 ≥0, x2 ≥0")
cons_b = [(1, -1, 3, -1), (1, 1, 9, -1), (-1, 1, 3, 1), (1, 1, 1.5, 1)]
_, _, _, verts_b = solve_lp_2d((0,0), cons_b)
print("Угловые точки области:")
for x, y in verts_b:
    print(f"  ({x:.2f}, {y:.2f})")

try:
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(12, 5))
    x = np.linspace(-1, 10, 400)
    y1 = 5 - x
    y2 = 3*x - 3
    ax1.plot(x, y1, label='x1+x2=5')
    ax1.plot(x, y2, label='3x1-x2=3')
    ax1.fill_between(x, 0, np.minimum(y1, y2), where=(x>=0)&(y1>=0)&(y2>=0), alpha=0.3)
    ax1.set_xlim(0, 6)
    ax1.set_ylim(0, 6)
    ax1.set_xlabel('x1')
    ax1.set_ylabel('x2')
    ax1.legend()
    ax1.grid()
    ax1.set_title('а) x1+x2≤5, 3x1-x2≤3, x1,x2≥0')
    xb = np.linspace(-1, 10, 400)
    y3 = xb - 3
    y4 = 9 - xb
    y5 = xb + 3
    y6 = 1.5 - xb
    ax2.plot(xb, y3, label='x1-x2=3')
    ax2.plot(xb, y4, label='x1+x2=9')
    ax2.plot(xb, y5, label='-x1+x2=3')
    ax2.plot(xb, y6, label='x1+x2=1.5')
    ymin = np.maximum(0, np.maximum(y3, y5))
    ymax = np.minimum(y4, 10)
    ax2.fill_between(xb, ymin, ymax, where=(xb>=0)&(ymax>ymin), alpha=0.3)
    ax2.set_xlim(0, 10)
    ax2.set_ylim(0, 10)
    ax2.set_xlabel('x1')
    ax2.set_ylabel('x2')
    ax2.legend()
    ax2.grid()
    ax2.set_title('б) система из 4 неравенств')
    plt.tight_layout()
    plt.show()
except:
    print("Для графиков нужна библиотека matplotlib")

print("\n2. Задача про мангалы")
print("x1 – кол-во угольных мангалов, x2 – кол-во газовых")
print("Целевая функция: max Z = ? (в условии не дана прибыль, предполагаем, что нужно максимизировать выпуск при ограничениях)")
print("Ограничения:")
print("  5*x1 + 8*x2 ≤ 2600  (производство)")
print("  0.8*x1 + 1.2*x2 ≤ 400 (сборка)")
print("  0.5*x1 + 0.5*x2 ≤ 200 (упаковка)")
print("  x1 ≥ 300, x2 ≥ 300 (контракт)")
print("  x1 ≥ 0, x2 ≥ 0")
print("\nТак как прибыль не указана, найдём максимально возможный выпуск (максимизируем x1+x2).")
c_m = (1, 1)
cons_m = [(5, 8, 2600, -1), (0.8, 1.2, 400, -1), (0.5, 0.5, 200, -1), (1, 0, 300, 1), (0, 1, 300, 1)]
x1_opt, x2_opt, val_opt, verts_m = solve_lp_2d(c_m, cons_m)
if x1_opt is not None:
    print(f"\nОптимальное решение: x1 = {x1_opt:.2f}, x2 = {x2_opt:.2f}")
    print(f"Максимальный суммарный выпуск: {val_opt:.2f} шт.")
else:
    print("Область допустимых решений пуста")