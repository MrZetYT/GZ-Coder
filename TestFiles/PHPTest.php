<?php

class Person {
    private $name;
    private $age;

    public function __construct($name, $age) {
        $this->name = $name;
        $this->age = $age;
    }

    public function greet() {
        echo "Hello, my name is {$this->name}\n";
    }

    public function haveBirthday() {
        $this->age += 1;
        echo "I am now {$this->age} years old\n";
    }
}

$alice = new Person("Alice", 30);
$alice->greet();
$alice->haveBirthday();
