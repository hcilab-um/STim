<%@ page import="console.AppStatus" %>



<div class="fieldcontain ${hasErrors(bean: appStatusInstance, field: 'lastUpdate', 'error')} required">
	<label for="lastUpdate">
		<g:message code="appStatus.lastUpdate.label" default="Last Update" />
		<span class="required-indicator">*</span>
	</label>
	<g:datePicker name="lastUpdate" precision="day"  value="${appStatusInstance?.lastUpdate}"  />
</div>

<div class="fieldcontain ${hasErrors(bean: appStatusInstance, field: 'message', 'error')} ">
	<label for="message">
		<g:message code="appStatus.message.label" default="Message" />
		
	</label>
	<g:textField name="message" value="${appStatusInstance?.message}"/>
</div>

<div class="fieldcontain ${hasErrors(bean: appStatusInstance, field: 'runningOK', 'error')} ">
	<label for="runningOK">
		<g:message code="appStatus.runningOK.label" default="Running OK" />
		
	</label>
	<g:checkBox name="runningOK" value="${appStatusInstance?.runningOK}" />
</div>

