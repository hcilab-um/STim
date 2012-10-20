
package console

import org.junit.*
import grails.test.mixin.*

@TestFor(AppStatusController)
@Mock(AppStatus)
class AppStatusControllerTests {

    def populateValidParams(params) 
	{
        assert params != null
		
        params["lastUpdate"] = new Date() 
		params["runningOK"] = true;
		params["message"] = "This is a test object"
    }

    void testIndex() {
        controller.index()
        assert "/appStatus/list" == response.redirectedUrl
    }

    void testList() {

        def model = controller.list()

        assert model.appStatusInstanceList.size() == 0
        assert model.appStatusInstanceTotal == 0
    }

    void testCreate() {
        def model = controller.create()

        assert model.appStatusInstance != null
    }

    void testSave() {
        controller.save()

        assert model.appStatusInstance != null
        assert view == '/appStatus/create'

        response.reset()

        populateValidParams(params)
        controller.save()

        assert response.redirectedUrl == '/appStatus/show/1'
        assert controller.flash.message != null
        assert AppStatus.count() == 1
    }

    void testShow() {
        controller.show()

        assert flash.message != null
        assert response.redirectedUrl == '/appStatus/list'

        populateValidParams(params)
        def appStatus = new AppStatus(params)

        assert appStatus.save() != null

        params.id = appStatus.id

        def model = controller.show()

        assert model.appStatusInstance == appStatus
    }

    void testEdit() {
        controller.edit()

        assert flash.message != null
        assert response.redirectedUrl == '/appStatus/list'

        populateValidParams(params)
        def appStatus = new AppStatus(params)

        assert appStatus.save() != null

        params.id = appStatus.id

        def model = controller.edit()

        assert model.appStatusInstance == appStatus
    }

    void testUpdate() {
        controller.update()

        assert flash.message != null
        assert response.redirectedUrl == '/appStatus/list'

        response.reset()

        populateValidParams(params)
        def appStatus = new AppStatus(params)

        assert appStatus.save() != null

        // test invalid parameters in update
        params.id = appStatus.id
		params["lastUpdate"] = "Jango Boogaloo"

        controller.update()

        assert view == "/appStatus/edit"
        assert model.appStatusInstance != null

        appStatus.clearErrors()

        populateValidParams(params)
        controller.update()

        assert response.redirectedUrl == "/appStatus/show/$appStatus.id"
        assert flash.message != null

        //test outdated version number
        response.reset()
        appStatus.clearErrors()

        populateValidParams(params)
        params.id = appStatus.id
        params.version = -1
        controller.update()

        assert view == "/appStatus/edit"
        assert model.appStatusInstance != null
        assert model.appStatusInstance.errors.getFieldError('version')
        assert flash.message != null
    }

    void testDelete() {
        controller.delete()
        assert flash.message != null
        assert response.redirectedUrl == '/appStatus/list'

        response.reset()

        populateValidParams(params)
        def appStatus = new AppStatus(params)

        assert appStatus.save() != null
        assert AppStatus.count() == 1

        params.id = appStatus.id

        controller.delete()

        assert AppStatus.count() == 0
        assert AppStatus.get(appStatus.id) == null
        assert response.redirectedUrl == '/appStatus/list'
    }
}
