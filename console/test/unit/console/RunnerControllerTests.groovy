package console



import org.junit.*
import grails.test.mixin.*

@TestFor(RunnerController)
@Mock(Runner)
class RunnerControllerTests {

    def populateValidParams(params) {
        assert params != null
        // TODO: Populate valid properties like...
        //params["name"] = 'someValidName'
    }

    void testIndex() {
        controller.index()
        assert "/runner/list" == response.redirectedUrl
    }

    void testList() {

        def model = controller.list()

        assert model.runnerInstanceList.size() == 0
        assert model.runnerInstanceTotal == 0
    }

    void testCreate() {
        def model = controller.create()

        assert model.runnerInstance != null
    }

    void testSave() {
        controller.save()

        assert model.runnerInstance != null
        assert view == '/runner/create'

        response.reset()

        populateValidParams(params)
        controller.save()

        assert response.redirectedUrl == '/runner/show/1'
        assert controller.flash.message != null
        assert Runner.count() == 1
    }

    void testShow() {
        controller.show()

        assert flash.message != null
        assert response.redirectedUrl == '/runner/list'

        populateValidParams(params)
        def runner = new Runner(params)

        assert runner.save() != null

        params.id = runner.id

        def model = controller.show()

        assert model.runnerInstance == runner
    }

    void testEdit() {
        controller.edit()

        assert flash.message != null
        assert response.redirectedUrl == '/runner/list'

        populateValidParams(params)
        def runner = new Runner(params)

        assert runner.save() != null

        params.id = runner.id

        def model = controller.edit()

        assert model.runnerInstance == runner
    }

    void testUpdate() {
        controller.update()

        assert flash.message != null
        assert response.redirectedUrl == '/runner/list'

        response.reset()

        populateValidParams(params)
        def runner = new Runner(params)

        assert runner.save() != null

        // test invalid parameters in update
        params.id = runner.id
        //TODO: add invalid values to params object

        controller.update()

        assert view == "/runner/edit"
        assert model.runnerInstance != null

        runner.clearErrors()

        populateValidParams(params)
        controller.update()

        assert response.redirectedUrl == "/runner/show/$runner.id"
        assert flash.message != null

        //test outdated version number
        response.reset()
        runner.clearErrors()

        populateValidParams(params)
        params.id = runner.id
        params.version = -1
        controller.update()

        assert view == "/runner/edit"
        assert model.runnerInstance != null
        assert model.runnerInstance.errors.getFieldError('version')
        assert flash.message != null
    }

    void testDelete() {
        controller.delete()
        assert flash.message != null
        assert response.redirectedUrl == '/runner/list'

        response.reset()

        populateValidParams(params)
        def runner = new Runner(params)

        assert runner.save() != null
        assert Runner.count() == 1

        params.id = runner.id

        controller.delete()

        assert Runner.count() == 0
        assert Runner.get(runner.id) == null
        assert response.redirectedUrl == '/runner/list'
    }
}
