
<%@ page import="console.AppStatus" %>
<!DOCTYPE html>
<html>
	<head>
		<meta name="layout" content="main">
		<g:set var="entityName" value="${message(code: 'appStatus.label', default: 'AppStatus')}" />
		<title><g:message code="default.list.label" args="[entityName]" /></title>
	</head>
	<body>
		<a href="#list-appStatus" class="skip" tabindex="-1"><g:message code="default.link.skip.label" default="Skip to content&hellip;"/></a>
		<div class="nav" role="navigation">
			<ul>
				<li><a class="home" href="${createLink(uri: '/')}"><g:message code="default.home.label"/></a></li>
				<li><g:link class="create" action="create"><g:message code="default.new.label" args="[entityName]" /></g:link></li>
			</ul>
		</div>
		<div id="list-appStatus" class="content scaffold-list" role="main">
			<h1><g:message code="default.list.label" args="[entityName]" /></h1>
			<g:if test="${flash.message}">
			<div class="message" role="status">${flash.message}</div>
			</g:if>
			<table>
				<thead>
					<tr>
					
						<g:sortableColumn property="lastUpdate" title="${message(code: 'appStatus.lastUpdate.label', default: 'Last Update')}" />
					
						<g:sortableColumn property="message" title="${message(code: 'appStatus.message.label', default: 'Message')}" />
					
						<g:sortableColumn property="runningOK" title="${message(code: 'appStatus.runningOK.label', default: 'Running OK')}" />
					
					</tr>
				</thead>
				<tbody>
				<g:each in="${appStatusInstanceList}" status="i" var="appStatusInstance">
					<tr class="${(i % 2) == 0 ? 'even' : 'odd'}">
					
						<td><g:link action="show" id="${appStatusInstance.id}">${fieldValue(bean: appStatusInstance, field: "lastUpdate")}</g:link></td>
					
						<td>${fieldValue(bean: appStatusInstance, field: "message")}</td>
					
						<td><g:formatBoolean boolean="${appStatusInstance.runningOK}" /></td>
					
					</tr>
				</g:each>
				</tbody>
			</table>
			<div class="pagination">
				<g:paginate total="${appStatusInstanceTotal}" />
			</div>
		</div>
	</body>
</html>
