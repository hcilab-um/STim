package console



import org.junit.*
import grails.test.mixin.*

@TestFor(JangoController)
@Mock(Jango)
class JangoControllerTests {

    def populateValidParams(params) {
        assert params != null
        // TODO: Populate valid properties like...
        params["name"] = "Test Name"
		params["birthday"] = new Date()
		params["weight"] = -1
    }

    void testIndex() {
        controller.index()
        assert "/jango/list" == response.redirectedUrl
    }

    void testList() {

        def model = controller.list()

        assert model.jangoInstanceList.size() == 0
        assert model.jangoInstanceTotal == 0
    }

    void testCreate() {
        def model = controller.create()

        assert model.jangoInstance != null
    }

    void testSave() {
        controller.save()

        assert model.jangoInstance != null
        assert view == '/jango/create'

        response.reset()

        populateValidParams(params)
        controller.save()

        assert response.redirectedUrl == '/jango/show/1'
        assert controller.flash.message != null
        assert Jango.count() == 1
    }

    void testShow() {
        controller.show()

        assert flash.message != null
        assert response.redirectedUrl == '/jango/list'

        populateValidParams(params)
        def jango = new Jango(params)

        assert jango.save() != null

        params.id = jango.id

        def model = controller.show()

        assert model.jangoInstance == jango
    }

    void testEdit() {
        controller.edit()

        assert flash.message != null
        assert response.redirectedUrl == '/jango/list'

        populateValidParams(params)
        def jango = new Jango(params)

        assert jango.save() != null

        params.id = jango.id

        def model = controller.edit()

        assert model.jangoInstance == jango
    }

    void testUpdate() {
        controller.update()

        assert flash.message != null
        assert response.redirectedUrl == '/jango/list'

        response.reset()

        populateValidParams(params)
        def jango = new Jango(params)

        assert jango.save() != null

        // test invalid parameters in update
        params.id = jango.id
		params["birthday"] = "Invalid Birthday"
        controller.update()

        assert view == "/jango/edit"
        assert model.jangoInstance != null

        jango.clearErrors()

        populateValidParams(params)
        controller.update()

        assert response.redirectedUrl == "/jango/show/$jango.id"
        assert flash.message != null

        //test outdated version number
        response.reset()
        jango.clearErrors()

        populateValidParams(params)
        params.id = jango.id
        params.version = -1
        controller.update()

        assert view == "/jango/edit"
        assert model.jangoInstance != null
        assert model.jangoInstance.errors.getFieldError('version')
        assert flash.message != null
    }

    void testDelete() {
        controller.delete()
        assert flash.message != null
        assert response.redirectedUrl == '/jango/list'

        response.reset()

        populateValidParams(params)
        def jango = new Jango(params)

        assert jango.save() != null
        assert Jango.count() == 1

        params.id = jango.id

        controller.delete()

        assert Jango.count() == 0
        assert Jango.get(jango.id) == null
        assert response.redirectedUrl == '/jango/list'
    }
}
