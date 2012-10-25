
<%@ page import="console.Jango" %>
<!DOCTYPE html>
<html>
	<head>
		<meta name="layout" content="main">
		<g:set var="entityName" value="${message(code: 'jango.label', default: 'Jango')}" />
		<title><g:message code="default.show.label" args="[entityName]" /></title>
	</head>
	<body>
		<a href="#show-jango" class="skip" tabindex="-1"><g:message code="default.link.skip.label" default="Skip to content&hellip;"/></a>
		<div class="nav" role="navigation">
			<ul>
				<li><a class="home" href="${createLink(uri: '/')}"><g:message code="default.home.label"/></a></li>
				<li><g:link class="list" action="list"><g:message code="default.list.label" args="[entityName]" /></g:link></li>
				<li><g:link class="create" action="create"><g:message code="default.new.label" args="[entityName]" /></g:link></li>
			</ul>
		</div>
		<div id="show-jango" class="content scaffold-show" role="main">
			<h1><g:message code="default.show.label" args="[entityName]" /></h1>
			<g:if test="${flash.message}">
			<div class="message" role="status">${flash.message}</div>
			</g:if>
			<ol class="property-list jango">
			
				<g:if test="${jangoInstance?.birthday}">
				<li class="fieldcontain">
					<span id="birthday-label" class="property-label"><g:message code="jango.birthday.label" default="Birthday" /></span>
					
						<span class="property-value" aria-labelledby="birthday-label"><g:formatDate date="${jangoInstance?.birthday}" /></span>
					
				</li>
				</g:if>
			
				<g:if test="${jangoInstance?.name}">
				<li class="fieldcontain">
					<span id="name-label" class="property-label"><g:message code="jango.name.label" default="Name" /></span>
					
						<span class="property-value" aria-labelledby="name-label"><g:fieldValue bean="${jangoInstance}" field="name"/></span>
					
				</li>
				</g:if>
			
				<g:if test="${jangoInstance?.weight}">
				<li class="fieldcontain">
					<span id="weight-label" class="property-label"><g:message code="jango.weight.label" default="Weight" /></span>
					
						<span class="property-value" aria-labelledby="weight-label"><g:fieldValue bean="${jangoInstance}" field="weight"/></span>
					
				</li>
				</g:if>
			
			</ol>
			<g:form>
				<fieldset class="buttons">
					<g:hiddenField name="id" value="${jangoInstance?.id}" />
					<g:link class="edit" action="edit" id="${jangoInstance?.id}"><g:message code="default.button.edit.label" default="Edit" /></g:link>
					<g:actionSubmit class="delete" action="delete" value="${message(code: 'default.button.delete.label', default: 'Delete')}" onclick="return confirm('${message(code: 'default.button.delete.confirm.message', default: 'Are you sure?')}');" />
				</fieldset>
			</g:form>
		</div>
	</body>
</html>
