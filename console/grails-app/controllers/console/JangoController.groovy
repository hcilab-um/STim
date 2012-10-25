package console

import org.springframework.dao.DataIntegrityViolationException

class JangoController {

    static allowedMethods = [save: "POST", update: "PUT", delete: "DELETE"]

    def index() {
        redirect(action: "list", params: params)
    }

    def list(Integer max) {
        params.max = Math.min(max ?: 10, 100)
        [jangoInstanceList: Jango.list(params), jangoInstanceTotal: Jango.count()]
    }

    def create() {
        [jangoInstance: new Jango(params)]
    }

    def save() {
        def jangoInstance = new Jango(params)
        if (!jangoInstance.save(flush: true)) {
            render(view: "create", model: [jangoInstance: jangoInstance])
            return
        }

        flash.message = message(code: 'default.created.message', args: [message(code: 'jango.label', default: 'Jango'), jangoInstance.id])
        redirect(action: "show", id: jangoInstance.id)
    }

    def show(Long id) {
        def jangoInstance = Jango.get(id)
        if (!jangoInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'jango.label', default: 'Jango'), id])
            redirect(action: "list")
            return
        }

        [jangoInstance: jangoInstance]
    }

    def edit(Long id) {
        def jangoInstance = Jango.get(id)
        if (!jangoInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'jango.label', default: 'Jango'), id])
            redirect(action: "list")
            return
        }

        [jangoInstance: jangoInstance]
    }

    def update(Long id, Long version) {
        def jangoInstance = Jango.get(id)
        if (!jangoInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'jango.label', default: 'Jango'), id])
            redirect(action: "list")
            return
        }

        if (version != null) {
            if (jangoInstance.version > version) {
                jangoInstance.errors.rejectValue("version", "default.optimistic.locking.failure",
                          [message(code: 'jango.label', default: 'Jango')] as Object[],
                          "Another user has updated this Jango while you were editing")
                render(view: "edit", model: [jangoInstance: jangoInstance])
                return
            }
        }

        jangoInstance.properties = params

        if (!jangoInstance.save(flush: true)) {
            render(view: "edit", model: [jangoInstance: jangoInstance])
            return
        }

        flash.message = message(code: 'default.updated.message', args: [message(code: 'jango.label', default: 'Jango'), jangoInstance.id])
        redirect(action: "show", id: jangoInstance.id)
    }

    def delete(Long id) {
        def jangoInstance = Jango.get(id)
        if (!jangoInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'jango.label', default: 'Jango'), id])
            redirect(action: "list")
            return
        }

        try {
            jangoInstance.delete(flush: true)
            flash.message = message(code: 'default.deleted.message', args: [message(code: 'jango.label', default: 'Jango'), id])
            redirect(action: "list")
        }
        catch (DataIntegrityViolationException e) {
            flash.message = message(code: 'default.not.deleted.message', args: [message(code: 'jango.label', default: 'Jango'), id])
            redirect(action: "show", id: id)
        }
    }
}
