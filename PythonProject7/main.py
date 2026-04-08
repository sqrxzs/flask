import numpy as np

profit = np.array([
    [12,  6, -2],   # П1
    [ 9,  7,  1],   # П2
    [ 8,  5,  4]    # П3
])

strategies = ["П1", "П2", "П3"]
states = ["Благоприятный", "Средний", "Неблагоприятный"]

alpha = 0.6

print("Матрица прибыли (млн руб.):")
for i, row in enumerate(profit):
    print(f"{strategies[i]}: {row}")
print("\n" + "="*50 + "\n")


min_profit_per_strategy = np.min(profit, axis=1)

wald_value = np.max(min_profit_per_strategy)

wald_index = np.argmax(min_profit_per_strategy)

print("Критерий Вальда (максимин):")
print(f"Минимальные прибыли по стратегиям: {min_profit_per_strategy}")
print(f"Лучшая гарантированная прибыль = {wald_value} млн руб.")
print(f"Оптимальная стратегия: {strategies[wald_index]}")
print("="*50 + "\n")


max_in_columns = np.max(profit, axis=0)
print("Максимумы по столбцам (лучший исход в каждом состоянии):", max_in_columns)

# Строим матрицу рисков: r_ij = max_j - a_ij
risk_matrix = max_in_columns - profit
print("Матрица рисков (упущенная выгода):")
for i, row in enumerate(risk_matrix):
    print(f"{strategies[i]}: {row}")

max_risk_per_strategy = np.max(risk_matrix, axis=1)
# Выбираем стратегию с минимальным из максимальных рисков
savage_value = np.min(max_risk_per_strategy)
savage_index = np.argmin(max_risk_per_strategy)

print(f"\nМаксимальные риски по стратегиям: {max_risk_per_strategy}")
print(f"Минимаксный риск = {savage_value} млн руб.")
print(f"Оптимальная стратегия: {strategies[savage_index]}")
print("="*50 + "\n")

max_profit_per_strategy = np.max(profit, axis=1)
min_profit_per_strategy = np.min(profit, axis=1)
hurwitz_values = alpha * max_profit_per_strategy + (1 - alpha) * min_profit_per_strategy

print("Критерий Гурвица (α = 0.6):")
for i, val in enumerate(hurwitz_values):
    print(f"{strategies[i]}: H = {alpha}*{max_profit_per_strategy[i]} + {1-alpha}*{min_profit_per_strategy[i]} = {val:.2f}")

hurwitz_index = np.argmax(hurwitz_values)
print(f"\nНаибольшее значение H = {hurwitz_values[hurwitz_index]:.2f} млн руб.")
print(f"Оптимальная стратегия: {strategies[hurwitz_index]}")
print("="*50 + "\n")


