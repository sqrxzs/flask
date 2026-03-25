import sys

def solve_investment(S0, n_years=4):

    step = 1.0
    max_S = S0
    S_values = [i for i in range(int(max_S) + 1)]

    f_next = {s: 0 for s in S_values}
    best_x = {}

    for year in range(n_years - 1, -1, -1):
        f_curr = {}
        best_x_year = {}
        for s in S_values:
            best_income = -1e9
            best_x_val = 0
            # перебираем x от 0 до s с шагом step
            for x in range(0, int(s) + 1):
                y = s - x
                income_now = 5*x + 3*y
                s_next = 0.1*x + 0.5*y

                s_next_int = int(round(s_next))
                if s_next_int > max_S:
                    s_next_int = max_S
                if s_next_int < 0:
                    s_next_int = 0
                total_income = income_now + f_next[s_next_int]
                if total_income > best_income + 1e-9:
                    best_income = total_income
                    best_x_val = x
            f_curr[s] = best_income
            best_x_year[s] = best_x_val
        f_next = f_curr
        best_x[year] = best_x_year

    s_current = S0
    plan = []
    for year in range(n_years):
        x_opt = best_x[year][int(s_current)]
        y_opt = s_current - x_opt
        plan.append((x_opt, y_opt))
        s_next = 0.1*x_opt + 0.5*y_opt
        s_current = s_next
    return f_next[int(S0)], plan


def dijkstra(graph, start, end):

    import heapq
    distances = {v: float('inf') for v in graph}
    distances[start] = 0
    previous = {v: None for v in graph}
    pq = [(0, start)]
    while pq:
        cur_dist, cur = heapq.heappop(pq)
        if cur_dist > distances[cur]:
            continue
        if cur == end:
            break
        for neighbor, weight in graph[cur].items():
            new_dist = cur_dist + weight
            if new_dist < distances[neighbor]:
                distances[neighbor] = new_dist
                previous[neighbor] = cur
                heapq.heappush(pq, (new_dist, neighbor))
    path = []
    cur = end
    while cur is not None:
        path.append(cur)
        cur = previous[cur]
    path.reverse()
    return distances[end], path

def build_graph_variant3():

    graph = {
        1: {2: 2, 3: 5},
        2: {1: 2, 4: 3, 5: 6},
        3: {1: 5, 4: 4, 6: 7},
        4: {2: 3, 3: 4, 5: 2, 7: 5},
        5: {2: 6, 4: 2, 8: 3},
        6: {3: 7, 7: 4, 9: 6},
        7: {4: 5, 6: 4, 8: 3, 10: 7},
        8: {5: 3, 7: 3, 10: 5},
        9: {6: 6, 10: 4},
        10: {7: 7, 8: 5, 9: 4}
    }
    return graph


if __name__ == "__main__":
    print("=" * 60)
    print("ЗАДАЧА 1: Распределение средств между предприятиями")
    print("=" * 60)
    S0 = 100
    max_income, distribution = solve_investment(S0)
    print(f"Начальная сумма: {S0}")
    print(f"Максимальный доход за 4 года: {max_income:.2f}")
    print("Распределение по годам (x_A, x_B):")
    for year, (x, y) in enumerate(distribution, 1):
        print(f"  Год {year}: A = {x:.2f}, B = {y:.2f}")

    print("\n" + "=" * 60)
    print("ЗАДАЧА 2: Кратчайший путь от 1 до 10 в графе (алгоритм Дейкстры)")
    print("=" * 60)
    graph = build_graph_variant3()
    print("Граф (ребра с весами):")
    for v, neighbors in graph.items():
        print(f"  {v}: {neighbors}")
    distance, path = dijkstra(graph, 1, 10)
    print(f"\nКратчайшее расстояние от 1 до 10: {distance}")
    print(f"Путь: {' -> '.join(map(str, path))}")