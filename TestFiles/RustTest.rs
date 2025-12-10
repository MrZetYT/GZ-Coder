struct Point {
    x: f64,
    y: f64,
}

impl Point {
    fn move_by(&mut self, dx: f64, dy: f64) {
        self.x += dx;
        self.y += dy;
    }

    fn distance_to_origin(&self) -> f64 {
        (self.x.powi(2) + self.y.powi(2)).sqrt()
    }
}

fn main() {
    let mut p = Point { x: 3.0, y: 4.0 };
    println!("Distance to origin: {}", p.distance_to_origin());
    p.move_by(1.0, -2.0);
    println!("Moved point: ({}, {})", p.x, p.y);
}
