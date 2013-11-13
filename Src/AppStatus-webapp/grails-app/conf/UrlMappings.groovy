class UrlMappings {

	static mappings = {
		"/$controller/$action?/$id?"{
			constraints {
				// apply constraints here
			}
//			action = [GET:"show", POST:"save", PUT:"update", DELETE:"remove"]
		}
//		"/runner/$id"(controller:"runner", parseRequest: true) {
//			action = [GET:"show", PUT:"update", DELETE:"delete", POST:"save"]
//		}
		"/"(view:"/index")
		"500"(view:'/error')
	}
}
