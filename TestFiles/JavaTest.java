public class Person {
    private String name;
    private int age;

    public Person(String name, int age) {
        this.name = name;
        this.age = age;
    }

    public void greet() {
        System.out.println("Hello, my name is " + name);
    }

    public void haveBirthday() {
        age += 1;
        System.out.println("I am now " + age + " years old");
    }

    public String getName() {
        return name;
    }

    public int getAge() {
        return age;
    }
}

public class Main {
    public static void main(String[] args) {
        Person alice = new Person("Alice", 30);
        alice.greet();
        alice.haveBirthday();
    }
}
