#include <stdio.h>
#include <math.h>

typedef struct {
    double x;
    double y;
} Point;

void move(Point* p, double dx, double dy) {
    p->x += dx;
    p->y += dy;
}

double distanceToOrigin(Point* p) {
    return sqrt(p->x * p->x + p->y * p->y);
}

int main() {
    Point p = {3.0, 4.0};
    printf("Distance to origin: %.2f\n", distanceToOrigin(&p));
    move(&p, 1.0, -2.0);
    printf("Moved point: (%.2f, %.2f)\n", p.x, p.y);
    return 0;
}
