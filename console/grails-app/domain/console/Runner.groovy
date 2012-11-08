package console

class Runner {

    static constraints = {
		age(min:18)
	}
	static expose = 'runner'
	String name
	double speed
	int age
	
	static belongsTo  = Race

}
