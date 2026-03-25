import numpy as np

a = np.array([180, 80, 40])
b = np.array([100, 100, 200])
C = np.array([[1, 3, 2],
              [4, 6, 1],
              [2, 3, 5]])

print("Транспортная задача (вариант 2)")
print("Запасы:", a)
print("Потребности:", b)
print("Матрица тарифов:")
print(C)
print()

if np.sum(a) != np.sum(b):
    print("Задача открытая, приводим к закрытой")
    if np.sum(a) > np.sum(b):
        b = np.append(b, np.sum(a) - np.sum(b))
        C = np.column_stack((C, np.zeros(len(a))))
    else:
        a = np.append(a, np.sum(b) - np.sum(a))
        C = np.row_stack((C, np.zeros(len(b))))
else:
    print("Задача закрытая")

m, n = len(a), len(b)

x = np.zeros((m, n))
a_copy = a.copy()
b_copy = b.copy()

while np.sum(a_copy) > 0 and np.sum(b_copy) > 0:
    min_cost = np.inf
    min_i, min_j = -1, -1
    for i in range(m):
        if a_copy[i] == 0:
            continue
        for j in range(n):
            if b_copy[j] == 0:
                continue
            if C[i, j] < min_cost:
                min_cost = C[i, j]
                min_i, min_j = i, j
    qty = min(a_copy[min_i], b_copy[min_j])
    x[min_i, min_j] = qty
    a_copy[min_i] -= qty
    b_copy[min_j] -= qty

print("Начальный опорный план (метод минимального элемента):")
print(x)

def find_cycle(x, i0, j0):
    m, n = x.shape
    filled = [(i, j) for i in range(m) for j in range(n) if x[i, j] > 0]

    path = [(i0, j0)]
    def dfs(i, j, prev_direction):
        if len(path) > 1 and i == i0 and j == j0:
            return True
        # Движение по горизонтали
        if prev_direction != 'h':
            for j2 in range(n):
                if j2 == j:
                    continue
                if x[i, j2] > 0 and (i, j2) not in path:
                    path.append((i, j2))
                    if dfs(i, j2, 'h'):
                        return True
                    path.pop()
        # Движение по вертикали
        if prev_direction != 'v':
            for i2 in range(m):
                if i2 == i:
                    continue
                if x[i2, j] > 0 and (i2, j) not in path:
                    path.append((i2, j))
                    if dfs(i2, j, 'v'):
                        return True
                    path.pop()
        return False
    dfs(i0, j0, None)
    return path

while True:
    u = np.full(m, np.nan)
    v = np.full(n, np.nan)
    u[0] = 0
    changed = True
    while changed:
        changed = False
        for i in range(m):
            for j in range(n):
                if x[i, j] > 0 and not np.isnan(u[i]) and np.isnan(v[j]):
                    v[j] = C[i, j] - u[i]
                    changed = True
                if x[i, j] > 0 and not np.isnan(v[j]) and np.isnan(u[i]):
                    u[i] = C[i, j] - v[j]
                    changed = True
    optimal = True
    min_delta = np.inf
    enter_i, enter_j = -1, -1
    for i in range(m):
        for j in range(n):
            if x[i, j] == 0 and not (np.isnan(u[i]) or np.isnan(v[j])):
                delta = C[i, j] - (u[i] + v[j])
                if delta < -1e-9:
                    optimal = False
                    if delta < min_delta:
                        min_delta = delta
                        enter_i, enter_j = i, j
    if optimal:
        break

    cycle = find_cycle(x, enter_i, enter_j)
    theta = np.inf
    for k, (i, j) in enumerate(cycle):
        if k % 2 == 1:
            if x[i, j] < theta:
                theta = x[i, j]
    for k, (i, j) in enumerate(cycle):
        if k % 2 == 0:
            x[i, j] += theta
        else:
            x[i, j] -= theta

print("\nОптимальный план перевозок:")
print(x)
total_cost = np.sum(x * C)
print(f"Минимальная стоимость перевозок: {total_cost:.2f}")

print("\nПроверка:")
print("Запасы поставщиков (план):", np.sum(x, axis=1))
print("Запасы по условию:", a)
print("Потребности потребителей (план):", np.sum(x, axis=0))
print("Потребности по условию:", b)