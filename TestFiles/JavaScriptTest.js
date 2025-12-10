class Person {
    constructor(name, age) {
        this.name = name;
        this.age = age;
    }

    greet() {
        console.log(`Hello, my name is ${this.name}`);
    }

    haveBirthday() {
        this.age += 1;
        console.log(`I am now ${this.age} years old`);
    }
}

function sum(a, b) {
    return a + b;
}

const alice = new Person("Alice", 30);
alice.greet();
console.log(sum(5, 7));
