
<%@ page import="console.Jango" %>
<!DOCTYPE html>
<html>
	<head>
		<meta name="layout" content="main">
		<g:set var="entityName" value="${message(code: 'jango.label', default: 'Jango')}" />
		<title><g:message code="default.list.label" args="[entityName]" /></title>
	</head>
	<body>
		<a href="#list-jango" class="skip" tabindex="-1"><g:message code="default.link.skip.label" default="Skip to content&hellip;"/></a>
		<div class="nav" role="navigation">
			<ul>
				<li><a class="home" href="${createLink(uri: '/')}"><g:message code="default.home.label"/></a></li>
				<li><g:link class="create" action="create"><g:message code="default.new.label" args="[entityName]" /></g:link></li>
			</ul>
		</div>
		<div id="list-jango" class="content scaffold-list" role="main">
			<h1><g:message code="default.list.label" args="[entityName]" /></h1>
			<g:if test="${flash.message}">
			<div class="message" role="status">${flash.message}</div>
			</g:if>
			<table>
				<thead>
					<tr>
					
						<g:sortableColumn property="birthday" title="${message(code: 'jango.birthday.label', default: 'Birthday')}" />
					
						<g:sortableColumn property="name" title="${message(code: 'jango.name.label', default: 'Name')}" />
					
						<g:sortableColumn property="weight" title="${message(code: 'jango.weight.label', default: 'Weight')}" />
					
					</tr>
				</thead>
				<tbody>
				<g:each in="${jangoInstanceList}" status="i" var="jangoInstance">
					<tr class="${(i % 2) == 0 ? 'even' : 'odd'}">
					
						<td><g:link action="show" id="${jangoInstance.id}">${fieldValue(bean: jangoInstance, field: "birthday")}</g:link></td>
					
						<td>${fieldValue(bean: jangoInstance, field: "name")}</td>
					
						<td>${fieldValue(bean: jangoInstance, field: "weight")}</td>
					
					</tr>
				</g:each>
				</tbody>
			</table>
			<div class="pagination">
				<g:paginate total="${jangoInstanceTotal}" />
			</div>
		</div>
	</body>
</html>
