package console

import org.springframework.dao.DataIntegrityViolationException

class AppStatusController {

    static allowedMethods = [save: "POST", update: "POST", delete: "POST"]

    def index() {
        redirect(action: "list", params: params)
    }

    def list(Integer max) {
        params.max = Math.min(max ?: 10, 100)
        [appStatusInstanceList: AppStatus.list(params), appStatusInstanceTotal: AppStatus.count()]
    }

    def create() {
        [appStatusInstance: new AppStatus(params)]
    }

    def save() {
        def appStatusInstance = new AppStatus(params)
        if (!appStatusInstance.save(flush: true)) {
            render(view: "create", model: [appStatusInstance: appStatusInstance])
            return
        }

        flash.message = message(code: 'default.created.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), appStatusInstance.id])
        redirect(action: "show", id: appStatusInstance.id)
    }

    def show(Long id) {
        def appStatusInstance = AppStatus.get(id)
        if (!appStatusInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), id])
            redirect(action: "list")
            return
        }

        [appStatusInstance: appStatusInstance]
    }

    def edit(Long id) {
        def appStatusInstance = AppStatus.get(id)
        if (!appStatusInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), id])
            redirect(action: "list")
            return
        }

        [appStatusInstance: appStatusInstance]
    }

    def update(Long id, Long version) {
        def appStatusInstance = AppStatus.get(id)
        if (!appStatusInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), id])
            redirect(action: "list")
            return
        }

        if (version != null) {
            if (appStatusInstance.version > version) {
                appStatusInstance.errors.rejectValue("version", "default.optimistic.locking.failure",
                          [message(code: 'appStatus.label', default: 'AppStatus')] as Object[],
                          "Another user has updated this AppStatus while you were editing")
                render(view: "edit", model: [appStatusInstance: appStatusInstance])
                return
            }
        }

        appStatusInstance.properties = params

        if (!appStatusInstance.save(flush: true)) {
            render(view: "edit", model: [appStatusInstance: appStatusInstance])
            return
        }

        flash.message = message(code: 'default.updated.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), appStatusInstance.id])
        redirect(action: "show", id: appStatusInstance.id)
    }

    def delete(Long id) {
        def appStatusInstance = AppStatus.get(id)
        if (!appStatusInstance) {
            flash.message = message(code: 'default.not.found.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), id])
            redirect(action: "list")
            return
        }

        try {
            appStatusInstance.delete(flush: true)
            flash.message = message(code: 'default.deleted.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), id])
            redirect(action: "list")
        }
        catch (DataIntegrityViolationException e) {
            flash.message = message(code: 'default.not.deleted.message', args: [message(code: 'appStatus.label', default: 'AppStatus'), id])
            redirect(action: "show", id: id)
        }
    }
}