import numpy as np
import matplotlib.pyplot as plt

def simplex(c, A, b):
    m, n = A.shape
    tableau = np.zeros((m + 1, n + m + 1))
    tableau[:-1, :n] = A
    tableau[:-1, n:n + m] = np.eye(m)
    tableau[:-1, -1] = b
    tableau[-1, :n] = -c
    while np.any(tableau[-1, :-1] < -1e-9):
        col = np.argmin(tableau[-1, :-1])
        ratios = []
        for i in range(m):
            if tableau[i, col] > 1e-9:
                ratios.append(tableau[i, -1] / tableau[i, col])
            else:
                ratios.append(np.inf)
        row = np.argmin(ratios)
        if ratios[row] == np.inf:
            return None
        pivot = tableau[row, col]
        tableau[row, :] /= pivot
        for i in range(m + 1):
            if i != row:
                tableau[i, :] -= tableau[i, col] * tableau[row, :]
    x = np.zeros(n)
    for i in range(m):
        col = np.where(tableau[i, :n] == 1)[0]
        if len(col) == 1:
            x[col[0]] = tableau[i, -1]
    return x

print("ВАРИАНТ 2\n")

print("1. Графическое решение систем неравенств")
print("а) x1 + x2 ≤ 5, 3x1 - x2 ≤ 3, x1 ≥ 0, x2 ≥ 0")
def find_vertices(constraints):
    lines = [(a, b, c) for a, b, c, _ in constraints]
    lines.append((1,0,0))
    lines.append((0,1,0))
    verts = []
    for i in range(len(lines)):
        for j in range(i+1, len(lines)):
            a1,b1,c1 = lines[i]
            a2,b2,c2 = lines[j]
            det = a1*b2 - a2*b1
            if abs(det) < 1e-9:
                continue
            x = (c1*b2 - c2*b1)/det
            y = (a1*c2 - a2*c1)/det
            feasible = True
            for a,b,c,s in constraints:
                val = a*x + b*y
                if s == -1:
                    if val > c + 1e-9:
                        feasible = False
                else:
                    if val < c - 1e-9:
                        feasible = False
            if feasible and x >= -1e-9 and y >= -1e-9:
                if not any(abs(x-vx)<1e-6 and abs(y-vy)<1e-6 for vx,vy in verts):
                    verts.append((x,y))
    return verts

cons_a = [(1,1,5,-1),(3,-1,3,-1)]
verts_a = find_vertices(cons_a)
print("Угловые точки:")
for x,y in verts_a:
    print(f"  ({x:.2f}, {y:.2f})")

print("\nб) x1 - x2 ≤ 3, x1 + x2 ≤ 9, -x1 + x2 ≥ 3, x1 + x2 ≥ 1.5, x1 ≥0, x2 ≥0")
cons_b = [(1,-1,3,-1),(1,1,9,-1),(-1,1,3,1),(1,1,1.5,1)]
verts_b = find_vertices(cons_b)
print("Угловые точки:")
for x,y in verts_b:
    print(f"  ({x:.2f}, {y:.2f})")

try:
    fig, (ax1,ax2) = plt.subplots(1,2,figsize=(12,5))
    x = np.linspace(-0.5,6,400)
    y1 = 5 - x
    y2 = 3*x - 3
    ax1.plot(x, y1, label='x1+x2=5')
    ax1.plot(x, y2, label='3x1-x2=3')
    ax1.fill_between(x, 0, np.minimum(y1,y2), where=(x>=0)&(y1>=0)&(y2>=0), alpha=0.3)
    ax1.set_xlim(0,6)
    ax1.set_ylim(0,6)
    ax1.set_xlabel('x1')
    ax1.set_ylabel('x2')
    ax1.legend()
    ax1.grid()
    ax1.set_title('а) x1+x2≤5, 3x1-x2≤3, x1,x2≥0')
    xb = np.linspace(-0.5,10,400)
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
    ax2.set_xlim(0,10)
    ax2.set_ylim(0,10)
    ax2.set_xlabel('x1')
    ax2.set_ylabel('x2')
    ax2.legend()
    ax2.grid()
    ax2.set_title('б) система из 4 неравенств')
    plt.tight_layout()
    plt.show()
except:
    print("Matplotlib не установлен, графики не построены")

print("\n2. Задача про мангалы")
print("Математическая модель:")
print("x1 – количество угольных мангалов")
print("x2 – количество газовых мангалов")
print("Целевая функция: max Z = x1 + x2 (суммарный выпуск)")
print("Ограничения:")
print("  5*x1 + 8*x2 ≤ 2600  (производство)")
print("  0.8*x1 + 1.2*x2 ≤ 400 (сборка)")
print("  0.5*x1 + 0.5*x2 ≤ 200 (упаковка)")
print("  x1 ≥ 300, x2 ≥ 300")
print("  x1, x2 ≥ 0")

A = np.array([[5, 8],
              [0.8, 1.2],
              [0.5, 0.5],
              [-1, 0],
              [0, -1]])
b = np.array([2600, 400, 200, -300, -300])
c = np.array([-1, -1])  # для симплекса минимизируем -Z, поэтому c = [-1,-1]

x_opt = simplex(c, A, b)
if x_opt is not None:
    print("\nРешение симплекс-методом:")
    print(f"x1 = {x_opt[0]:.2f}, x2 = {x_opt[1]:.2f}")
    print(f"Суммарный выпуск Z = {x_opt[0] + x_opt[1]:.2f}")
else:
    print("Задача не имеет допустимого решения")