<%@ page import="console.Jango" %>



<div class="fieldcontain ${hasErrors(bean: jangoInstance, field: 'birthday', 'error')} required">
	<label for="birthday">
		<g:message code="jango.birthday.label" default="Birthday" />
		<span class="required-indicator">*</span>
	</label>
	<g:datePicker name="birthday" precision="day"  value="${jangoInstance?.birthday}"  />
</div>

<div class="fieldcontain ${hasErrors(bean: jangoInstance, field: 'name', 'error')} ">
	<label for="name">
		<g:message code="jango.name.label" default="Name" />
		
	</label>
	<g:textField name="name" value="${jangoInstance?.name}"/>
</div>

<div class="fieldcontain ${hasErrors(bean: jangoInstance, field: 'weight', 'error')} required">
	<label for="weight">
		<g:message code="jango.weight.label" default="Weight" />
		<span class="required-indicator">*</span>
	</label>
	<g:field name="weight" value="${fieldValue(bean: jangoInstance, field: 'weight')}" required=""/>
</div>

