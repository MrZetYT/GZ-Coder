#include <iostream>
#include <cmath>
#include <string>

class Point {
public:
    Point(double x, double y) : x(x), y(y) {}

    void move(double dx, double dy) {
        x += dx;
        y += dy;
    }

    double distanceToOrigin() const {
        return std::sqrt(x*x + y*y);
    }

private:
    double x;
    double y;
};

int main() {
    Point p(3.0, 4.0);
    std::cout << "Distance to origin: " << p.distanceToOrigin() << std::endl;
    p.move(1.0, -2.0);
    std::cout << "Point moved." << std::endl;
    return 0;
}
