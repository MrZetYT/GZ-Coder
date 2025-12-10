interface Shape {
    area(): number;
    perimeter(): number;
}

class Rectangle implements Shape {
    constructor(private width: number, private height: number) {}

    area(): number {
        return this.width * this.height;
    }

    perimeter(): number {
        return 2 * (this.width + this.height);
    }
}

function createRectangle(width: number, height: number): Rectangle {
    return new Rectangle(width, height);
}

const rect = createRectangle(5, 10);
console.log(`Area: ${rect.area()}, Perimeter: ${rect.perimeter()}`);
