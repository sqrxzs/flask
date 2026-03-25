import numpy as np
import matplotlib.pyplot as plt

L = 10.0
T = 2.0
Nx = 200
Nt = 2000
a = 1.0

h = 2 * L / Nx
tau = T / Nt
sigma = a * tau / h**2

if sigma > 0.5:
    print(f"sigma = {sigma:.3f} > 0.5, схема неустойчива")
else:
    print(f"sigma = {sigma:.3f} <= 0.5, схема устойчива")

x = np.linspace(-L, L, Nx+1)
u = np.exp(-x**2)
u_new = u.copy()

for j in range(Nt):
    for i in range(1, Nx):
        u_new[i] = u[i] + sigma * (u[i-1] - 2*u[i] + u[i+1])
    u[:] = u_new[:]

plt.plot(x, u, label=f't = {T}')
plt.xlabel('x')
plt.ylabel('u(x,t)')
plt.title('Явная схема для уравнения теплопроводности')
plt.grid()
plt.show()