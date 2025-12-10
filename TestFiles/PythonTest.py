class Point:
    def __init__(self, x, y):
        self.x = x
        self.y = y

    def move(self, dx, dy):
        self.x += dx
        self.y += dy

    def distance_to_origin(self):
        return (self.x ** 2 + self.y ** 2) ** 0.5


def greet(name):
    print(f"Hello, {name}!")


if __name__ == "__main__":
    p = Point(3, 4)
    print(p.distance_to_origin())
    p.move(1, -2)
    greet("Alice")
